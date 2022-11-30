namespace MyLab.FileStorage.Tools;

public partial class Md5Ex
{
    public class Md5Context
    {
        /// <summary>
        /// state (ABCD)
        /// </summary>
        public uint[] State { get; } = new uint[4];
        /// <summary>
        /// number of bits, modulo 2^64 (lsb first)
        /// </summary>
        public uint[] Count { get; } = new uint[2];
        /// <summary>
        /// input buffer 
        /// </summary>
        public byte[] Buffer { get; } = new byte[64];
        
        public void Clear()
        {
            Array.Clear(State, 0, State.Length);
            Array.Clear(Count, 0, Count.Length);
            Array.Clear(Buffer, 0, Buffer.Length);
        }

        public byte[] SerializeAsync()
        {
            var resultBuff = new byte[
                sizeof(uint) * State.Length +
                sizeof(uint) * Count.Length +
                Buffer.Length
            ];

            for (int i = 0; i < State.Length; i++)
            {
                var bytes = BitConverter.GetBytes(State[i]);
                bytes.CopyTo(resultBuff, i * sizeof(uint));
            }

            for (int i = 0; i < Count.Length; i++)
            {
                var bytes = BitConverter.GetBytes(Count[i]);
                bytes.CopyTo(resultBuff, sizeof(uint) * State.Length + i * sizeof(uint));
            }
            
            Buffer.CopyTo(resultBuff, sizeof(uint) * State.Length + sizeof(uint) * Count.Length);

            return resultBuff;
        }

        public static Md5Context DeserializeAsync(byte[] initialData)
        {
            var ctx = new Md5Context();

            for (int i = 0; i < ctx.State.Length; i++)
                ctx.State[i] = BitConverter.ToUInt32(initialData, sizeof(uint) * i);

            for (int i = 0; i < ctx.Count.Length; i++)
                ctx.Count[i] = BitConverter.ToUInt32(initialData, sizeof(uint) * ctx.State.Length + sizeof(uint) * i);

            Array.Copy(initialData, sizeof(uint) * ctx.State.Length + sizeof(uint) * ctx.Count.Length, ctx.Buffer, 0, ctx.Buffer.Length);

            return ctx;
        }
    }
}