using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Runtime.VM.Program.Listener;

namespace Mineral.Common.Runtime.VM.Program
{
    public class Memory : IProgramListenerAware
    {
        #region Field
        private static readonly int CHUNK_SIZE = 1024;
        private static readonly int WORD_SIZE = 32;

        private List<byte[]> chunks = new List<byte[]>();
        private int soft_size = 0;
        private IProgramListener program_listener = null;
        #endregion


        #region Property
        public int Size => soft_size;

        public int InternalSize
        {
            get { return this.chunks.Count * CHUNK_SIZE; }
        }

        public List<byte[]> Chunks
        {
            get { return new List<byte[]>(this.chunks); }
        }
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private int CaptureMax(int chunk_index, int chunk_offset, int size, byte[] src, int src_pos)
        {
            byte[] chunk = this.chunks[chunk_index];
            int to_capture = Math.Min(size, chunk.Length - chunk_offset);

            Array.Copy(src, src_pos, chunk, chunk_offset, to_capture);
            return to_capture;
        }

        private int GrabMax(int chunk_index, int chunk_offset, int size, byte[] dest, int dest_pos)
        {
            byte[] chunk = this.chunks[chunk_index];
            int to_grab = Math.Min(size, chunk.Length - chunk_offset);

            Array.Copy(chunk, chunk_offset, dest, dest_pos, to_grab);

            return to_grab;
        }

        private void AddChunks(int num)
        {
            for (int i = 0; i < num; ++i)
            {
                this.chunks.Add(new byte[CHUNK_SIZE]);
            }
        }
        #endregion


        #region External Method
        public void SetProgramListener(IProgramListener listener)
        {
            this.program_listener = listener;
        }

        public byte[] Read(int address, int size)
        {
            if (size <= 0)
            {
                return new byte[0];
            }

            Extend(address, size);
            byte[] data = new byte[size];

            int chunk_index = address / CHUNK_SIZE;
            int chunk_offset = address % CHUNK_SIZE;

            int to_grab = data.Length;
            int start = 0;

            while (to_grab > 0)
            {
                int copied = GrabMax(chunk_index, chunk_offset, to_grab, data, start);

                ++chunk_index;
                chunk_offset = 0;

                to_grab -= copied;
                start += copied;
            }

            return data;
        }

        public void Write(int address, byte[] data, int data_size, bool limited)
        {
            if (data.Length < data_size)
            {
                data_size = data.Length;
            }

            if (!limited)
            {
                Extend(address, data_size);
            }

            int chunk_index = address / CHUNK_SIZE;
            int chunk_offset = address % CHUNK_SIZE;

            int to_capture = 0;
            if (limited)
            {
                to_capture = (address + data_size > this.soft_size) ? this.soft_size - address : data_size;
            }
            else
            {
                to_capture = data_size;
            }

            int start = 0;
            while (to_capture > 0)
            {
                int captured = CaptureMax(chunk_index, chunk_offset, to_capture, data, start);

                // capture next chunk
                ++chunk_index;
                chunk_offset = 0;

                // mark remind
                to_capture -= captured;
                start += captured;
            }

            if (this.program_listener != null)
            {
                this.program_listener.OnMemoryWrite(address, data, data_size);
            }
        }

        public void ExtendAndWrite(int address, int alloc_size, byte[] data)
        {
            Extend(address, alloc_size);
            Write(address, data, data.Length, false);
        }

        public void Extend(int address, int size)
        {
            if (size <= 0)
            {
                return;
            }

            int new_size = address + size;
            int to_allocate = new_size - InternalSize;
            if (to_allocate > 0)
            {
                AddChunks((int)Math.Ceiling((double)to_allocate / CHUNK_SIZE));
            }

            to_allocate = new_size - this.soft_size;
            if (to_allocate > 0)
            {
                to_allocate = (int)Math.Ceiling((double)to_allocate / WORD_SIZE) * WORD_SIZE;
                this.soft_size += to_allocate;

                if (this.program_listener != null)
                {
                    this.program_listener.OnMemoryExtend(to_allocate);
                }
            }
        }

        public DataWord ReadWord(int address)
        {
            return new DataWord(Read(address, 32));
        }

        public byte ReadByte(int address)
        {
            int chunk_index = address / CHUNK_SIZE;
            int chunk_offset = address % CHUNK_SIZE;
            byte[] chunk = this.chunks[chunk_index];

            return chunk[chunk_offset];
        }

        public override string ToString()
        {
            StringBuilder memory_data = new StringBuilder();
            StringBuilder first_line = new StringBuilder();
            StringBuilder second_line = new StringBuilder();

            for (int i = 0; i < this.soft_size; ++i)
            {
                byte value = ReadByte(i);

                string character = ((byte)0x20 <= value && value <= (byte)0x7e) ? new string(new char[] { (char)value }) : "?";
                first_line.Append(character)
                          .Append("");
                second_line.Append(Helper.ToHexString(value))
                          .Append(" ");

                if ((i + 1) % 8 == 0)
                {
                    String tmp = string.Format("%4s", Helper.ToHexString((byte)(i - 7))).Replace(" ", "0");
                    memory_data.Append("")
                               .Append(tmp)
                               .Append(" ");
                    memory_data.Append(first_line)
                               .Append(" ");
                    memory_data.Append(second_line);

                    if (i + 1 < this.soft_size)
                    {
                        memory_data.Append("\n");
                    }

                    first_line.Length = 0;
                    second_line.Length = 0;
                }
            }

            return memory_data.ToString();
        }
        #endregion
    }
}
