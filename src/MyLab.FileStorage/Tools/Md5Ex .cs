using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace MyLab.FileStorage.Tools
{
    public partial class Md5Ex
    {
        // Current context
        private readonly Md5Context _context;
        // Last hash result
        private readonly byte[] _digest = new byte[16];
        // True if HashCore has been called
        private bool _hashCoreCalled;
        // True if HashFinal has been called
        private bool _hashFinalCalled;

        /// <summary>
        /// Returns the hash as an array of bytes.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "Matching .NET behavior by throwing here.")]
        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes", Justification = "Matching .NET behavior by throwing NullReferenceException.")]
        public byte[] Hash
        {
            get
            {
                if (!_hashCoreCalled)
                {
                    throw new NullReferenceException();
                }
                if (!_hashFinalCalled)
                {
                    // Note: Not CryptographicUnexpectedOperationException because that can't be instantiated on Silverlight 4
                    throw new CryptographicException("Hash must be finalized before the hash value is retrieved.");
                }

                return _digest;
            }
        }

        public Md5Context Context => _context;

        // Return size of hash in bits.
        public int HashSize => _digest.Length * 8;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public Md5Ex()
        {
            _context = new Md5Context();
            Md5Init(_context);

            _hashCoreCalled = false;
            _hashFinalCalled = false;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public Md5Ex(Md5Context initialContext)
        {
            _context = initialContext;
            
            _hashCoreCalled = false;
            _hashFinalCalled = false;
        }

        /// <summary>
        /// Updates the hash code with the data provided.
        /// </summary>
        /// <param name="array">Data to hash.</param>
        public void AppendData(byte[] array)
        {
            if (null == array)
                throw new ArgumentNullException(nameof(array));
            if (_hashFinalCalled)
                throw new CryptographicException("Hash not valid for use in specified state.");
            _hashCoreCalled = true;

            Md5Update(_context, array, 0, (uint)array.Length);
        }

        /// <summary>
        /// Finalizes the hash code and returns it.
        /// </summary>
        public byte[] FinalHash()
        {
            _hashFinalCalled = true;
            Md5Final(_digest, _context);
            return Hash;
        }
    }
}
