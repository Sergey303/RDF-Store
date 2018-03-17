namespace ConsoleEndpoint
{
    using System.Collections.Generic;
    using System.IO;

    using ConsoleEndpoint.Interfaces;

    using Polar.DB;

    public class Nametable : INametable
    {
        private UniversalSequence<int> ids;

        private UniversalSequence<int> offsets;

        private Dictionary<string, int> buffer = new Dictionary<string, int>();

        public Nametable(Stream idsStream, Stream offsetsStream)
        {
                this.ids = new UniversalSequence<int>(PolarExtension.GetPolarType<string>(), idsStream);

                this.offsets = new UniversalSequence<int>(typeof(long).GetPolarType(), offsetsStream);
            int nom = 0;
            this.ids.Scan(
                (off, ob) =>
                    {
                        this.buffer.Add((string)ob, nom);
                        nom++;
                        return true;
                    });
        }

        public long LongCount() => this.offsets.Count();

        /// <summary>
        /// получение строки по коду
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public string GetString(int code) =>
            code >= 0 && code < this.offsets.Count()
                ? (string)this.ids.GetElement((long)this.offsets.GetElement(this.offsets.ElementOffset(code)))
                : null;

        public void Clear()
        {
            this.buffer.Clear();
            this.offsets.Clear();
            this.ids.Clear();
        }

        /// <summary>
        ///  Получение кода по строке
        /// </summary>
        /// <param name="s"></param>
        /// <param name="exists"></param>
        /// <returns></returns>
        public int GetCode(string s, out bool exists) => (exists = this.buffer.TryGetValue(s, out int nom)) ? nom : 0;

        /// <summary>
        ///  Запись кода по строке
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public int GetSetCode(string s)
        {
            if (this.buffer.TryGetValue(s, out var nom)) return nom;
            nom = (int)this.ids.Count();
            long offset = this.ids.AppendElement(s);
            this.offsets.AppendElement(offset);
            this.buffer.Add(s, nom);
            return nom;
        }
    }
}