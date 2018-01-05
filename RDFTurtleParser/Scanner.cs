using System;
using System.Collections;
using System.IO;

namespace RDFTurtleParser {

public class Token {
	public int kind;    // token kind
	public long pos;     // token position in bytes in the source text (starting at 0)
	public int charPos;  // token position in characters in the source text (starting at 0)
	public int col;     // token column (starting at 1)
	public int line;    // token line (starting at 1)
	public string val;  // token value
	public Token next;  // ML 2005-03-11 Tokens are kept in linked list
}

//-----------------------------------------------------------------------------------
// Buffer
//-----------------------------------------------------------------------------------
public class Buffer {
	// This Buffer supports the following cases:
	// 1) seekable stream (file)
	//    a) whole stream in buffer
	//    b) part of stream in buffer
	// 2) non seekable stream (network, console)

	public const int EOF = char.MaxValue + 1;
	const int MIN_BUFFER_LENGTH = 1024; // 1KB
	const int MAX_BUFFER_LENGTH = MIN_BUFFER_LENGTH * 64; // 64KB
	byte[] buf;         // input buffer
	long bufStart;       // position of first byte in buffer relative to input stream
	long bufLen;         // length of buffer
	long fileLen;        // length of input stream (may change if the stream is no file)
	long bufPos;         // current position in buffer
	Stream stream;      // input stream (seekable)
	bool isUserStream;  // was the stream opened by the user?
	
	public Buffer (Stream s, bool isUserStream) {
		stream = s; this.isUserStream = isUserStream;
		
		if (stream.CanSeek) {
			fileLen = (long) stream.Length;
		    bufLen = fileLen > MAX_BUFFER_LENGTH ? MAX_BUFFER_LENGTH : (int)fileLen;
			bufStart = Int32.MaxValue; // nothing in the buffer so far
		} else {
			fileLen = bufLen = bufStart = 0;
		}

		buf = new byte[(bufLen>0) ? bufLen : MIN_BUFFER_LENGTH];
		if (fileLen > 0) Pos = 0; // setup buffer to position 0 (start)
		else bufPos = 0; // index 0 is already after the file, thus Pos = 0 is invalid
		if (bufLen == fileLen && stream.CanSeek) Close();
	}
	
	protected Buffer(Buffer b) { // called in UTF8Buffer constructor
		buf = b.buf;
		bufStart = b.bufStart;
		bufLen = b.bufLen;
		fileLen = b.fileLen;
		bufPos = b.bufPos;
		stream = b.stream;
		// keep destructor from closing the stream
		b.stream = null;
		isUserStream = b.isUserStream;
	}

	~Buffer() { Close(); }
	
	protected void Close() {
		if (!isUserStream && stream != null) {
			stream.Close();
			stream = null;
		}
	}
	
	public virtual int Read () {
		if (bufPos < bufLen) {
			return buf[bufPos++];
		} else if (Pos < fileLen) {
			Pos = Pos; // shift buffer start to Pos
			return buf[bufPos++];
		} else if (stream != null && !stream.CanSeek && ReadNextStreamChunk() > 0) {
			return buf[bufPos++];
		} else {
			return EOF;
		}
	}

	public int Peek () {
		long curPos = Pos;
		int ch = Read();
		Pos = curPos;
		return ch;
	}
	
	// beg .. begin, zero-based, inclusive, in byte
	// end .. end, zero-based, exclusive, in byte
	public string GetString (int beg, int end) {
		int len = 0;
		char[] buf = new char[end - beg];
		long  oldPos = Pos;
		Pos = beg;
		while (Pos < end) buf[len++] = (char) Read();
		Pos = oldPos;
		return new String(buf, 0, len);
	}

	public long Pos {
		get { return bufPos + bufStart; }
		set {
			if (value >= fileLen && stream != null && !stream.CanSeek) {
				// Wanted position is after buffer and the stream
				// is not seek-able e.g. network or console,
				// thus we have to read the stream manually till
				// the wanted position is in sight.
				while (value >= fileLen && ReadNextStreamChunk() > 0);
			}

			if (value < 0 || value > fileLen) {
				throw new FatalError("buffer out of bounds access, position: " + value);
			}

			if (value >= bufStart && value < bufStart + bufLen) { // already in buffer
				bufPos = value - bufStart;
			} else if (stream != null) { // must be swapped in
				stream.Seek(value, SeekOrigin.Begin);
				bufLen = stream.Read(buf, 0, buf.Length);
				bufStart = value; bufPos = 0;
			} else {
				// set the position to the end of the file, Pos will return fileLen.
				bufPos = fileLen - bufStart;
			}
		}
	}
	
	// Read the next chunk of bytes from the stream, increases the buffer
	// if needed and updates the fields fileLen and bufLen.
	// Returns the number of bytes read.
	private long ReadNextStreamChunk() {
		long free = buf.Length - bufLen;
		if (free == 0) {
			// in the case of a growing input stream
			// we can neither seek in the stream, nor can we
			// foresee the maximum length, thus we must adapt
			// the buffer size on demand.
			byte[] newBuf = new byte[bufLen * 2];
			Array.Copy(buf, newBuf, bufLen);
			buf = newBuf;
			free = bufLen;
		}
	    if (stream.Length <= bufLen) return 0;
	    stream.Seek(bufLen, SeekOrigin.Begin);
        long count = free + bufLen > stream.Length ? stream.Length - bufLen : free;
	    for (int i = 0; i < count; i++)
	    {                                                         
	        buf[i]=(byte) stream.ReadByte();                      
	    }
		//int read = stream.Read(buf, bufLen, free);
		//if (read > 0) {
			fileLen = bufLen = (bufLen + count);
			return count;
		//}
		// end of stream reached
	//	return 0;
	}
}

//-----------------------------------------------------------------------------------
// UTF8Buffer
//-----------------------------------------------------------------------------------
public class UTF8Buffer: Buffer {
	public UTF8Buffer(Buffer b): base(b) {}

	public override int Read() {
		int ch;
		do {
			ch = base.Read();
			// until we find a utf8 start (0xxxxxxx or 11xxxxxx)
		} while ((ch >= 128) && ((ch & 0xC0) != 0xC0) && (ch != EOF));
		if (ch < 128 || ch == EOF) {
			// nothing to do, first 127 chars are the same in ascii and utf8
			// 0xxxxxxx or end of file character
		} else if ((ch & 0xF0) == 0xF0) {
			// 11110xxx 10xxxxxx 10xxxxxx 10xxxxxx
			int c1 = ch & 0x07; ch = base.Read();
			int c2 = ch & 0x3F; ch = base.Read();
			int c3 = ch & 0x3F; ch = base.Read();
			int c4 = ch & 0x3F;
			ch = (((((c1 << 6) | c2) << 6) | c3) << 6) | c4;
		} else if ((ch & 0xE0) == 0xE0) {
			// 1110xxxx 10xxxxxx 10xxxxxx
			int c1 = ch & 0x0F; ch = base.Read();
			int c2 = ch & 0x3F; ch = base.Read();
			int c3 = ch & 0x3F;
			ch = (((c1 << 6) | c2) << 6) | c3;
		} else if ((ch & 0xC0) == 0xC0) {
			// 110xxxxx 10xxxxxx
			int c1 = ch & 0x1F; ch = base.Read();
			int c2 = ch & 0x3F;
			ch = (c1 << 6) | c2;
		}
		return ch;
	}
}

//-----------------------------------------------------------------------------------
// Scanner
//-----------------------------------------------------------------------------------
public class Scanner {
	const char EOL = '\n';
	const int eofSym = 0; /* pdt */
	const int maxT = 29;
	const int noSym = 29;


	public Buffer buffer; // scanner buffer
	
	Token t;          // current token
	int ch;           // current input character
	long pos;          // byte position of current character
	int charPos;      // position by unicode characters starting with 0
	int col;          // column number of current character
	int line;         // line number of current character
	int oldEols;      // EOLs that appeared in a comment;
	static readonly Hashtable start; // maps first token character to start state

	Token tokens;     // list of tokens already peeked (first token is a dummy)
	Token pt;         // current peek token
	
	char[] tval = new char[128]; // text of current token
	long tlen;         // length of current token
	
	static Scanner() {
		start = new Hashtable(128);
		for (int i = 65; i <= 90; ++i) start[i] = 30;
		for (int i = 99; i <= 101; ++i) start[i] = 30;
		for (int i = 103; i <= 111; ++i) start[i] = 30;
		for (int i = 113; i <= 115; ++i) start[i] = 30;
		for (int i = 117; i <= 122; ++i) start[i] = 30;
		for (int i = 192; i <= 192; ++i) start[i] = 30;
		for (int i = 214; i <= 214; ++i) start[i] = 30;
		for (int i = 216; i <= 216; ++i) start[i] = 30;
		for (int i = 246; i <= 246; ++i) start[i] = 30;
		for (int i = 248; i <= 248; ++i) start[i] = 30;
		for (int i = 767; i <= 767; ++i) start[i] = 30;
		for (int i = 880; i <= 880; ++i) start[i] = 30;
		for (int i = 893; i <= 893; ++i) start[i] = 30;
		for (int i = 895; i <= 895; ++i) start[i] = 30;
		for (int i = 4096; i <= 4096; ++i) start[i] = 30;
		for (int i = 8191; i <= 8191; ++i) start[i] = 30;
		for (int i = 8204; i <= 8205; ++i) start[i] = 30;
		for (int i = 8304; i <= 8304; ++i) start[i] = 30;
		for (int i = 8591; i <= 8591; ++i) start[i] = 30;
		for (int i = 11264; i <= 11264; ++i) start[i] = 30;
		for (int i = 12271; i <= 12271; ++i) start[i] = 30;
		for (int i = 12289; i <= 12289; ++i) start[i] = 30;
		for (int i = 55295; i <= 55295; ++i) start[i] = 30;
		for (int i = 61439; i <= 61439; ++i) start[i] = 30;
		for (int i = 63744; i <= 63744; ++i) start[i] = 30;
		for (int i = 64975; i <= 64975; ++i) start[i] = 30;
		for (int i = 65008; i <= 65008; ++i) start[i] = 30;
		for (int i = 65533; i <= 65533; ++i) start[i] = 30;
		for (int i = 49; i <= 57; ++i) start[i] = 31;
		for (int i = 43; i <= 43; ++i) start[i] = 32;
		for (int i = 45; i <= 45; ++i) start[i] = 32;
		start[48] = 33; 
		start[60] = 1; 
		start[58] = 34; 
		start[95] = 7; 
		start[64] = 10; 
		start[46] = 117; 
		start[34] = 35; 
		start[39] = 36; 
		start[91] = 118; 
		start[98] = 119; 
		start[112] = 120; 
		start[59] = 110; 
		start[44] = 111; 
		start[97] = 121; 
		start[93] = 112; 
		start[40] = 113; 
		start[41] = 114; 
		start[94] = 115; 
		start[116] = 122; 
		start[102] = 123; 
		start[Buffer.EOF] = -1;

	}
	
	public Scanner (string fileName) {
		try {
			Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			buffer = new Buffer(stream, false);
			Init();
		} catch (IOException) {
			throw new FatalError("Cannot open file " + fileName);
		}
	}
	
	public Scanner (Stream s) {
		buffer = new Buffer(s, true);
		Init();
	}
	
	void Init() {
		pos = -1; line = 1; col = 0; charPos = -1;
		oldEols = 0;
		NextCh();
		if (ch == 0xEF) { // check optional byte order mark for UTF-8
			NextCh(); int ch1 = ch;
			NextCh(); int ch2 = ch;
			if (ch1 != 0xBB || ch2 != 0xBF) {
				throw new FatalError(String.Format("illegal byte order mark: EF {0,2:X} {1,2:X}", ch1, ch2));
			}
			buffer = new UTF8Buffer(buffer); col = 0; charPos = -1;
			NextCh();
		}
		pt = tokens = new Token();  // first token is a dummy
	}
	
	void NextCh() {
		if (oldEols > 0) { ch = EOL; oldEols--; } 
		else {
			pos = buffer.Pos;
			// buffer reads unicode chars, if UTF8 has been detected
			ch = buffer.Read(); col++; charPos++;
			// replace isolated '\r' by '\n' in order to make
			// eol handling uniform across Windows, Unix and Mac
			if (ch == '\r' && buffer.Peek() != '\n') ch = EOL;
			if (ch == EOL) { line++; col = 0; }
		}

	}

	void AddCh() {
		if (tlen >= tval.Length) {
			char[] newBuf = new char[2 * tval.Length];
			Array.Copy(tval, 0, newBuf, 0, tval.Length);
			tval = newBuf;
		}
		if (ch != Buffer.EOF) {
			tval[tlen++] = (char) ch;
			NextCh();
		}
	}



	bool Comment0() {
	    int level = 1;
	    long pos0 = pos;
	    int line0 = line, col0 = col, charPos0 = charPos;
	    NextCh();
			for(;;) {
				if (ch == 10) {
					level--;
					if (level == 0) { oldEols = line - line0; NextCh(); return true; }
					NextCh();
				} else if (ch == Buffer.EOF) return false;
				else NextCh();
			}
	}


	void CheckLiteral() {
		switch (t.val) {
			case "@prefix": t.kind = 15; break;
			case "@base": t.kind = 16; break;
			default: break;
		}
	}

	Token NextToken() {
		while (ch == ' ' ||
			ch >= 9 && ch <= 10 || ch == 13
		) NextCh();
		if (ch == '#' && Comment0()) return NextToken();
		int recKind = noSym;
		long recEnd = pos;
		t = new Token();
		t.pos = pos; t.col = col; t.line = line; t.charPos = charPos;
		int state;
		if (start.ContainsKey(ch)) { state = (int) start[ch]; }
		else { state = 0; }
		tlen = 0; AddCh();
		
		switch (state) {
			case -1: { t.kind = eofSym; break; } // NextCh already done
			case 0: {
				if (recKind != noSym) {
					tlen = recEnd - t.pos;
					SetScannerBehindT();
				}
				t.kind = recKind; break;
			} // NextCh already done
			case 1:
				if (ch <= '!' || ch >= '#' && ch <= ';' || ch == '=' || ch >= '?' && ch <= '[' || ch == ']' || ch == '_' || ch >= 'a' && ch <= 'z' || ch >= '~' && ch <= 65535) {AddCh(); goto case 1;}
				else if (ch == '>') {AddCh(); goto case 2;}
				else {goto case 0;}
			case 2:
				{t.kind = 1; break;}
			case 3:
				recEnd = pos; recKind = 3;
				if (ch >= '-' && ch <= '.' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 37;}
				else if (ch == ':') {AddCh(); goto case 38;}
				else if (ch == '%') {AddCh(); goto case 39;}
				else if (ch == 92) {AddCh(); goto case 40;}
				else {t.kind = 3; break;}
			case 4:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F') {AddCh(); goto case 5;}
				else {goto case 0;}
			case 5:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F') {AddCh(); goto case 3;}
				else {goto case 0;}
			case 6:
				if (ch >= '!' && ch <= '/' || ch == ';' || ch == '=' || ch >= '?' && ch <= '@' || ch == '_' || ch == '|' || ch == '~') {AddCh(); goto case 3;}
				else {goto case 0;}
			case 7:
				if (ch == ':') {AddCh(); goto case 8;}
				else {goto case 0;}
			case 8:
				if (ch == '.' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z' || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch == 767 || ch == 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 9;}
				else {goto case 0;}
			case 9:
				recEnd = pos; recKind = 4;
				if (ch >= '-' && ch <= '.' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 43;}
				else {t.kind = 4; break;}
			case 10:
				if (ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 11;}
				else {goto case 0;}
			case 11:
				recEnd = pos; recKind = 5;
				if (ch == '-' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 11;}
				else {t.kind = 5; t.val = new String(tval, 0, (int)tlen); CheckLiteral(); return t;}
			case 12:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 14;}
				else if (ch == '+' || ch == '-') {AddCh(); goto case 13;}
				else {goto case 0;}
			case 13:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 14;}
				else {goto case 0;}
			case 14:
				recEnd = pos; recKind = 8;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 14;}
				else {t.kind = 8; break;}
			case 15:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 17;}
				else if (ch == '+' || ch == '-') {AddCh(); goto case 16;}
				else {goto case 0;}
			case 16:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 17;}
				else {goto case 0;}
			case 17:
				recEnd = pos; recKind = 8;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 17;}
				else {t.kind = 8; break;}
			case 18:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 20;}
				else if (ch == '+' || ch == '-') {AddCh(); goto case 19;}
				else {goto case 0;}
			case 19:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 20;}
				else {goto case 0;}
			case 20:
				recEnd = pos; recKind = 8;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 20;}
				else {t.kind = 8; break;}
			case 21:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '!' || ch >= '#' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 21;}
				else if (ch == '"') {AddCh(); goto case 22;}
				else if (ch == 92) {AddCh(); goto case 45;}
				else {goto case 0;}
			case 22:
				{t.kind = 9; break;}
			case 23:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '&' || ch >= '(' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 23;}
				else if (ch == 39) {AddCh(); goto case 24;}
				else if (ch == 92) {AddCh(); goto case 46;}
				else {goto case 0;}
			case 24:
				{t.kind = 10; break;}
			case 25:
				if (ch <= '&' || ch >= '(' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 25;}
				else if (ch == 39) {AddCh(); goto case 47;}
				else if (ch == 92) {AddCh(); goto case 48;}
				else {goto case 0;}
			case 26:
				{t.kind = 11; break;}
			case 27:
				if (ch <= '!' || ch >= '#' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 27;}
				else if (ch == '"') {AddCh(); goto case 49;}
				else if (ch == 92) {AddCh(); goto case 50;}
				else {goto case 0;}
			case 28:
				{t.kind = 12; break;}
			case 29:
				{t.kind = 13; break;}
			case 30:
				if (ch >= '-' && ch <= '.' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch == ':') {AddCh(); goto case 52;}
				else {goto case 0;}
			case 31:
				recEnd = pos; recKind = 6;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 31;}
				else if (ch == '.') {AddCh(); goto case 53;}
				else if (ch == 'e') {AddCh(); goto case 18;}
				else {t.kind = 6; break;}
			case 32:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 31;}
				else if (ch == '.') {AddCh(); goto case 54;}
				else {goto case 0;}
			case 33:
				recEnd = pos; recKind = 6;
				if (ch == '-' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'd' || ch >= 'f' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch >= '0' && ch <= '9') {AddCh(); goto case 55;}
				else if (ch == ':') {AddCh(); goto case 52;}
				else if (ch == '.') {AddCh(); goto case 56;}
				else if (ch == 'e') {AddCh(); goto case 57;}
				else {t.kind = 6; break;}
			case 34:
				recEnd = pos; recKind = 2;
				if (ch == '.' || ch >= '0' && ch <= ':' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z' || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch == 767 || ch == 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 3;}
				else if (ch == '%') {AddCh(); goto case 4;}
				else if (ch == 92) {AddCh(); goto case 6;}
				else {t.kind = 2; break;}
			case 35:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '!' || ch >= '#' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 21;}
				else if (ch == '"') {AddCh(); goto case 59;}
				else if (ch == 92) {AddCh(); goto case 45;}
				else {goto case 0;}
			case 36:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '&' || ch >= '(' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 23;}
				else if (ch == 39) {AddCh(); goto case 60;}
				else if (ch == 92) {AddCh(); goto case 46;}
				else {goto case 0;}
			case 37:
				recEnd = pos; recKind = 3;
				if (ch >= '-' && ch <= '.' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 41;}
				else if (ch == ':') {AddCh(); goto case 42;}
				else if (ch == '%') {AddCh(); goto case 39;}
				else if (ch == 92) {AddCh(); goto case 40;}
				else {t.kind = 3; break;}
			case 38:
				recEnd = pos; recKind = 3;
				if (ch >= '-' && ch <= '.' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 41;}
				else if (ch == ':') {AddCh(); goto case 42;}
				else if (ch == '%') {AddCh(); goto case 39;}
				else if (ch == 92) {AddCh(); goto case 40;}
				else {t.kind = 3; break;}
			case 39:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F') {AddCh(); goto case 61;}
				else {goto case 0;}
			case 40:
				if (ch >= '!' && ch <= '/' || ch == ';' || ch == '=' || ch >= '?' && ch <= '@' || ch == '_' || ch == '|' || ch == '~') {AddCh(); goto case 62;}
				else {goto case 0;}
			case 41:
				recEnd = pos; recKind = 3;
				if (ch >= '-' && ch <= '.' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 41;}
				else if (ch == ':') {AddCh(); goto case 42;}
				else if (ch == '%') {AddCh(); goto case 39;}
				else if (ch == 92) {AddCh(); goto case 40;}
				else {t.kind = 3; break;}
			case 42:
				recEnd = pos; recKind = 3;
				if (ch >= '-' && ch <= '.' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 41;}
				else if (ch == ':') {AddCh(); goto case 42;}
				else if (ch == '%') {AddCh(); goto case 39;}
				else if (ch == 92) {AddCh(); goto case 40;}
				else {t.kind = 3; break;}
			case 43:
				recEnd = pos; recKind = 4;
				if (ch >= '-' && ch <= '.' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 44;}
				else {t.kind = 4; break;}
			case 44:
				recEnd = pos; recKind = 4;
				if (ch >= '-' && ch <= '.' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 44;}
				else {t.kind = 4; break;}
			case 45:
				if (ch == '"' || ch == 39 || ch == 92 || ch == 'b' || ch == 'f' || ch == 'n' || ch == 'r' || ch == 't') {AddCh(); goto case 21;}
				else if (ch == 'u') {AddCh(); goto case 63;}
				else {goto case 0;}
			case 46:
				if (ch == '"' || ch == 39 || ch == 92 || ch == 'b' || ch == 'f' || ch == 'n' || ch == 'r' || ch == 't') {AddCh(); goto case 23;}
				else if (ch == 'u') {AddCh(); goto case 64;}
				else {goto case 0;}
			case 47:
				if (ch <= '&' || ch >= '(' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 25;}
				else if (ch == 39) {AddCh(); goto case 65;}
				else if (ch == 92) {AddCh(); goto case 48;}
				else {goto case 0;}
			case 48:
				if (ch == '"' || ch == 39 || ch == 92 || ch == 'b' || ch == 'f' || ch == 'n' || ch == 'r' || ch == 't') {AddCh(); goto case 25;}
				else if (ch == 'u') {AddCh(); goto case 66;}
				else {goto case 0;}
			case 49:
				if (ch <= '!' || ch >= '#' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 27;}
				else if (ch == '"') {AddCh(); goto case 67;}
				else if (ch == 92) {AddCh(); goto case 50;}
				else {goto case 0;}
			case 50:
				if (ch == '"' || ch == 39 || ch == 92 || ch == 'b' || ch == 'f' || ch == 'n' || ch == 'r' || ch == 't') {AddCh(); goto case 27;}
				else if (ch == 'u') {AddCh(); goto case 68;}
				else {goto case 0;}
			case 51:
				if (ch >= '-' && ch <= '.' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch == ':') {AddCh(); goto case 69;}
				else {goto case 0;}
			case 52:
				recEnd = pos; recKind = 2;
				if (ch == '.' || ch >= '0' && ch <= ':' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z' || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch == 767 || ch == 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 3;}
				else if (ch == '%') {AddCh(); goto case 4;}
				else if (ch == 92) {AddCh(); goto case 6;}
				else {t.kind = 2; break;}
			case 53:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 70;}
				else if (ch == 'e') {AddCh(); goto case 12;}
				else {goto case 0;}
			case 54:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 71;}
				else {goto case 0;}
			case 55:
				recEnd = pos; recKind = 6;
				if (ch == '-' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'd' || ch >= 'f' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch >= '0' && ch <= '9') {AddCh(); goto case 55;}
				else if (ch == ':') {AddCh(); goto case 69;}
				else if (ch == '.') {AddCh(); goto case 56;}
				else if (ch == 'e') {AddCh(); goto case 57;}
				else {t.kind = 6; break;}
			case 56:
				if (ch >= '-' && ch <= '.' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'd' || ch >= 'f' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch >= '0' && ch <= '9') {AddCh(); goto case 72;}
				else if (ch == ':') {AddCh(); goto case 69;}
				else if (ch == 'e') {AddCh(); goto case 73;}
				else {goto case 0;}
			case 57:
				if (ch == '.' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch >= '0' && ch <= '9') {AddCh(); goto case 74;}
				else if (ch == '+') {AddCh(); goto case 19;}
				else if (ch == ':') {AddCh(); goto case 69;}
				else if (ch == '-') {AddCh(); goto case 75;}
				else {goto case 0;}
			case 58:
				recEnd = pos; recKind = 7;
				if (ch >= '-' && ch <= '.' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'd' || ch >= 'f' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch >= '0' && ch <= '9') {AddCh(); goto case 58;}
				else if (ch == ':') {AddCh(); goto case 69;}
				else if (ch == 'e') {AddCh(); goto case 76;}
				else {t.kind = 7; break;}
			case 59:
				recEnd = pos; recKind = 9;
				if (ch == '"') {AddCh(); goto case 27;}
				else {t.kind = 9; break;}
			case 60:
				recEnd = pos; recKind = 10;
				if (ch == 39) {AddCh(); goto case 25;}
				else {t.kind = 10; break;}
			case 61:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F') {AddCh(); goto case 77;}
				else {goto case 0;}
			case 62:
				recEnd = pos; recKind = 3;
				if (ch >= '-' && ch <= '.' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 41;}
				else if (ch == ':') {AddCh(); goto case 42;}
				else if (ch == '%') {AddCh(); goto case 39;}
				else if (ch == 92) {AddCh(); goto case 40;}
				else {t.kind = 3; break;}
			case 63:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F') {AddCh(); goto case 78;}
				else {goto case 0;}
			case 64:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F') {AddCh(); goto case 79;}
				else {goto case 0;}
			case 65:
				if (ch <= '&' || ch >= '(' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 25;}
				else if (ch == 39) {AddCh(); goto case 26;}
				else if (ch == 92) {AddCh(); goto case 48;}
				else {goto case 0;}
			case 66:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F') {AddCh(); goto case 80;}
				else {goto case 0;}
			case 67:
				if (ch <= '!' || ch >= '#' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 27;}
				else if (ch == '"') {AddCh(); goto case 28;}
				else if (ch == 92) {AddCh(); goto case 50;}
				else {goto case 0;}
			case 68:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F') {AddCh(); goto case 81;}
				else {goto case 0;}
			case 69:
				recEnd = pos; recKind = 2;
				if (ch == '.' || ch >= '0' && ch <= ':' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z' || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch == 767 || ch == 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 3;}
				else if (ch == '%') {AddCh(); goto case 4;}
				else if (ch == 92) {AddCh(); goto case 6;}
				else {t.kind = 2; break;}
			case 70:
				recEnd = pos; recKind = 7;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 70;}
				else if (ch == 'e') {AddCh(); goto case 12;}
				else {t.kind = 7; break;}
			case 71:
				recEnd = pos; recKind = 7;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 71;}
				else if (ch == 'e') {AddCh(); goto case 15;}
				else {t.kind = 7; break;}
			case 72:
				recEnd = pos; recKind = 7;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 72;}
				else if (ch >= '-' && ch <= '.' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'd' || ch >= 'f' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch == 'e') {AddCh(); goto case 73;}
				else if (ch == ':') {AddCh(); goto case 69;}
				else {t.kind = 7; break;}
			case 73:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 82;}
				else if (ch == '+') {AddCh(); goto case 13;}
				else if (ch == '.' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch == '-') {AddCh(); goto case 83;}
				else if (ch == ':') {AddCh(); goto case 69;}
				else {goto case 0;}
			case 74:
				recEnd = pos; recKind = 8;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 74;}
				else if (ch >= '-' && ch <= '.' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch == ':') {AddCh(); goto case 69;}
				else {t.kind = 8; break;}
			case 75:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 74;}
				else if (ch >= '-' && ch <= '.' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch == ':') {AddCh(); goto case 69;}
				else {goto case 0;}
			case 76:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 84;}
				else if (ch == '+') {AddCh(); goto case 16;}
				else if (ch == '.' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch == '-') {AddCh(); goto case 85;}
				else if (ch == ':') {AddCh(); goto case 69;}
				else {goto case 0;}
			case 77:
				recEnd = pos; recKind = 3;
				if (ch >= '-' && ch <= '.' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 41;}
				else if (ch == ':') {AddCh(); goto case 42;}
				else if (ch == '%') {AddCh(); goto case 39;}
				else if (ch == 92) {AddCh(); goto case 40;}
				else {t.kind = 3; break;}
			case 78:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F') {AddCh(); goto case 86;}
				else {goto case 0;}
			case 79:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F') {AddCh(); goto case 87;}
				else {goto case 0;}
			case 80:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F') {AddCh(); goto case 88;}
				else {goto case 0;}
			case 81:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F') {AddCh(); goto case 89;}
				else {goto case 0;}
			case 82:
				recEnd = pos; recKind = 8;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 82;}
				else if (ch >= '-' && ch <= '.' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch == ':') {AddCh(); goto case 69;}
				else {t.kind = 8; break;}
			case 83:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 82;}
				else if (ch >= '-' && ch <= '.' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch == ':') {AddCh(); goto case 69;}
				else {goto case 0;}
			case 84:
				recEnd = pos; recKind = 8;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 84;}
				else if (ch >= '-' && ch <= '.' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch == ':') {AddCh(); goto case 69;}
				else {t.kind = 8; break;}
			case 85:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 84;}
				else if (ch >= '-' && ch <= '.' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch == ':') {AddCh(); goto case 69;}
				else {goto case 0;}
			case 86:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F') {AddCh(); goto case 90;}
				else {goto case 0;}
			case 87:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F') {AddCh(); goto case 91;}
				else {goto case 0;}
			case 88:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F') {AddCh(); goto case 92;}
				else {goto case 0;}
			case 89:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F') {AddCh(); goto case 93;}
				else {goto case 0;}
			case 90:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F') {AddCh(); goto case 94;}
				else {goto case 0;}
			case 91:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F') {AddCh(); goto case 95;}
				else {goto case 0;}
			case 92:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F') {AddCh(); goto case 96;}
				else {goto case 0;}
			case 93:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F') {AddCh(); goto case 97;}
				else {goto case 0;}
			case 94:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '!' || ch >= '#' && ch <= '/' || ch >= ':' && ch <= '@' || ch >= 'G' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 21;}
				else if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F') {AddCh(); goto case 98;}
				else if (ch == '"') {AddCh(); goto case 22;}
				else if (ch == 92) {AddCh(); goto case 45;}
				else {goto case 0;}
			case 95:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '&' || ch >= '(' && ch <= '/' || ch >= ':' && ch <= '@' || ch >= 'G' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 23;}
				else if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F') {AddCh(); goto case 99;}
				else if (ch == 39) {AddCh(); goto case 24;}
				else if (ch == 92) {AddCh(); goto case 46;}
				else {goto case 0;}
			case 96:
				if (ch <= '&' || ch >= '(' && ch <= '/' || ch >= ':' && ch <= '@' || ch >= 'G' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 25;}
				else if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F') {AddCh(); goto case 100;}
				else if (ch == 39) {AddCh(); goto case 47;}
				else if (ch == 92) {AddCh(); goto case 48;}
				else {goto case 0;}
			case 97:
				if (ch <= '!' || ch >= '#' && ch <= '/' || ch >= ':' && ch <= '@' || ch >= 'G' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 27;}
				else if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F') {AddCh(); goto case 101;}
				else if (ch == '"') {AddCh(); goto case 49;}
				else if (ch == 92) {AddCh(); goto case 50;}
				else {goto case 0;}
			case 98:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '!' || ch >= '#' && ch <= '/' || ch >= ':' && ch <= '@' || ch >= 'G' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 21;}
				else if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F') {AddCh(); goto case 102;}
				else if (ch == '"') {AddCh(); goto case 22;}
				else if (ch == 92) {AddCh(); goto case 45;}
				else {goto case 0;}
			case 99:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '&' || ch >= '(' && ch <= '/' || ch >= ':' && ch <= '@' || ch >= 'G' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 23;}
				else if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F') {AddCh(); goto case 103;}
				else if (ch == 39) {AddCh(); goto case 24;}
				else if (ch == 92) {AddCh(); goto case 46;}
				else {goto case 0;}
			case 100:
				if (ch <= '&' || ch >= '(' && ch <= '/' || ch >= ':' && ch <= '@' || ch >= 'G' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 25;}
				else if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F') {AddCh(); goto case 104;}
				else if (ch == 39) {AddCh(); goto case 47;}
				else if (ch == 92) {AddCh(); goto case 48;}
				else {goto case 0;}
			case 101:
				if (ch <= '!' || ch >= '#' && ch <= '/' || ch >= ':' && ch <= '@' || ch >= 'G' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 27;}
				else if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F') {AddCh(); goto case 105;}
				else if (ch == '"') {AddCh(); goto case 49;}
				else if (ch == 92) {AddCh(); goto case 50;}
				else {goto case 0;}
			case 102:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '!' || ch >= '#' && ch <= '/' || ch >= ':' && ch <= '@' || ch >= 'G' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 21;}
				else if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F') {AddCh(); goto case 106;}
				else if (ch == '"') {AddCh(); goto case 22;}
				else if (ch == 92) {AddCh(); goto case 45;}
				else {goto case 0;}
			case 103:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '&' || ch >= '(' && ch <= '/' || ch >= ':' && ch <= '@' || ch >= 'G' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 23;}
				else if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F') {AddCh(); goto case 107;}
				else if (ch == 39) {AddCh(); goto case 24;}
				else if (ch == 92) {AddCh(); goto case 46;}
				else {goto case 0;}
			case 104:
				if (ch <= '&' || ch >= '(' && ch <= '/' || ch >= ':' && ch <= '@' || ch >= 'G' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 25;}
				else if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F') {AddCh(); goto case 108;}
				else if (ch == 39) {AddCh(); goto case 47;}
				else if (ch == 92) {AddCh(); goto case 48;}
				else {goto case 0;}
			case 105:
				if (ch <= '!' || ch >= '#' && ch <= '/' || ch >= ':' && ch <= '@' || ch >= 'G' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 27;}
				else if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F') {AddCh(); goto case 109;}
				else if (ch == '"') {AddCh(); goto case 49;}
				else if (ch == 92) {AddCh(); goto case 50;}
				else {goto case 0;}
			case 106:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '!' || ch >= '#' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 21;}
				else if (ch == '"') {AddCh(); goto case 22;}
				else if (ch == 92) {AddCh(); goto case 45;}
				else {goto case 0;}
			case 107:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '&' || ch >= '(' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 23;}
				else if (ch == 39) {AddCh(); goto case 24;}
				else if (ch == 92) {AddCh(); goto case 46;}
				else {goto case 0;}
			case 108:
				if (ch <= '&' || ch >= '(' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 25;}
				else if (ch == 39) {AddCh(); goto case 47;}
				else if (ch == 92) {AddCh(); goto case 48;}
				else {goto case 0;}
			case 109:
				if (ch <= '!' || ch >= '#' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 27;}
				else if (ch == '"') {AddCh(); goto case 49;}
				else if (ch == 92) {AddCh(); goto case 50;}
				else {goto case 0;}
			case 110:
				{t.kind = 19; break;}
			case 111:
				{t.kind = 20; break;}
			case 112:
				{t.kind = 23; break;}
			case 113:
				{t.kind = 24; break;}
			case 114:
				{t.kind = 25; break;}
			case 115:
				if (ch == '^') {AddCh(); goto case 116;}
				else {goto case 0;}
			case 116:
				{t.kind = 26; break;}
			case 117:
				recEnd = pos; recKind = 14;
				if (ch >= '-' && ch <= '.' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch >= '0' && ch <= '9') {AddCh(); goto case 58;}
				else if (ch == ':') {AddCh(); goto case 52;}
				else {t.kind = 14; break;}
			case 118:
				recEnd = pos; recKind = 22;
				if (ch == ']') {AddCh(); goto case 29;}
				else {t.kind = 22; break;}
			case 119:
				if (ch >= '-' && ch <= '.' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'b' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch == ':') {AddCh(); goto case 52;}
				else if (ch == 'a') {AddCh(); goto case 124;}
				else {goto case 0;}
			case 120:
				if (ch >= '-' && ch <= '.' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'q' || ch >= 's' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch == ':') {AddCh(); goto case 52;}
				else if (ch == 'r') {AddCh(); goto case 125;}
				else {goto case 0;}
			case 121:
				recEnd = pos; recKind = 21;
				if (ch >= '-' && ch <= '.' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch == ':') {AddCh(); goto case 52;}
				else {t.kind = 21; break;}
			case 122:
				if (ch >= '-' && ch <= '.' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'q' || ch >= 's' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch == ':') {AddCh(); goto case 52;}
				else if (ch == 'r') {AddCh(); goto case 126;}
				else {goto case 0;}
			case 123:
				if (ch >= '-' && ch <= '.' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'b' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch == ':') {AddCh(); goto case 52;}
				else if (ch == 'a') {AddCh(); goto case 127;}
				else {goto case 0;}
			case 124:
				if (ch >= '-' && ch <= '.' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'r' || ch >= 't' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch == ':') {AddCh(); goto case 69;}
				else if (ch == 's') {AddCh(); goto case 128;}
				else {goto case 0;}
			case 125:
				if (ch >= '-' && ch <= '.' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'd' || ch >= 'f' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch == ':') {AddCh(); goto case 69;}
				else if (ch == 'e') {AddCh(); goto case 129;}
				else {goto case 0;}
			case 126:
				if (ch >= '-' && ch <= '.' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 't' || ch >= 'v' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch == ':') {AddCh(); goto case 69;}
				else if (ch == 'u') {AddCh(); goto case 130;}
				else {goto case 0;}
			case 127:
				if (ch >= '-' && ch <= '.' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'k' || ch >= 'm' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch == ':') {AddCh(); goto case 69;}
				else if (ch == 'l') {AddCh(); goto case 131;}
				else {goto case 0;}
			case 128:
				if (ch >= '-' && ch <= '.' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'd' || ch >= 'f' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch == ':') {AddCh(); goto case 69;}
				else if (ch == 'e') {AddCh(); goto case 132;}
				else {goto case 0;}
			case 129:
				if (ch >= '-' && ch <= '.' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'e' || ch >= 'g' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch == ':') {AddCh(); goto case 69;}
				else if (ch == 'f') {AddCh(); goto case 133;}
				else {goto case 0;}
			case 130:
				if (ch >= '-' && ch <= '.' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'd' || ch >= 'f' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch == ':') {AddCh(); goto case 69;}
				else if (ch == 'e') {AddCh(); goto case 134;}
				else {goto case 0;}
			case 131:
				if (ch >= '-' && ch <= '.' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'r' || ch >= 't' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch == ':') {AddCh(); goto case 69;}
				else if (ch == 's') {AddCh(); goto case 135;}
				else {goto case 0;}
			case 132:
				recEnd = pos; recKind = 17;
				if (ch >= '-' && ch <= '.' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch == ':') {AddCh(); goto case 69;}
				else {t.kind = 17; break;}
			case 133:
				if (ch >= '-' && ch <= '.' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'h' || ch >= 'j' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch == ':') {AddCh(); goto case 69;}
				else if (ch == 'i') {AddCh(); goto case 136;}
				else {goto case 0;}
			case 134:
				recEnd = pos; recKind = 27;
				if (ch >= '-' && ch <= '.' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch == ':') {AddCh(); goto case 69;}
				else {t.kind = 27; break;}
			case 135:
				if (ch >= '-' && ch <= '.' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'd' || ch >= 'f' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch == ':') {AddCh(); goto case 69;}
				else if (ch == 'e') {AddCh(); goto case 137;}
				else {goto case 0;}
			case 136:
				if (ch >= '-' && ch <= '.' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'w' || ch >= 'y' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch == ':') {AddCh(); goto case 69;}
				else if (ch == 'x') {AddCh(); goto case 138;}
				else {goto case 0;}
			case 137:
				recEnd = pos; recKind = 28;
				if (ch >= '-' && ch <= '.' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch == ':') {AddCh(); goto case 69;}
				else {t.kind = 28; break;}
			case 138:
				recEnd = pos; recKind = 18;
				if (ch >= '-' && ch <= '.' || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z' || ch == 183 || ch == 192 || ch == 214 || ch == 216 || ch == 246 || ch == 248 || ch >= 767 && ch <= 768 || ch >= 879 && ch <= 880 || ch == 893 || ch == 895 || ch == 4096 || ch == 8191 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8304 || ch == 8591 || ch == 11264 || ch == 12271 || ch == 12289 || ch == 55295 || ch == 61439 || ch == 63744 || ch == 64975 || ch == 65008 || ch == 65533) {AddCh(); goto case 51;}
				else if (ch == ':') {AddCh(); goto case 69;}
				else {t.kind = 18; break;}

		}
		t.val = new String(tval, 0, (int)tlen);
		return t;
	}
	
	private void SetScannerBehindT() {
		buffer.Pos = t.pos;
		NextCh();
		line = t.line; col = t.col; charPos = t.charPos;
		for (int i = 0; i < tlen; i++) NextCh();
	}
	
	// get the next token (possibly a token already seen during peeking)
	public Token Scan () {
		if (tokens.next == null) {
			return NextToken();
		} else {
			pt = tokens = tokens.next;
			return tokens;
		}
	}

	// peek for the next token, ignore pragmas
	public Token Peek () {
		do {
			if (pt.next == null) {
				pt.next = NextToken();
			}
			pt = pt.next;
		} while (pt.kind > maxT); // skip pragmas
	
		return pt;
	}

	// make sure that peeking starts at the current scan position
	public void ResetPeek () { pt = tokens; }

} // end Scanner
}