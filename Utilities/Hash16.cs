using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace FunkkModInstaller.Utilities

{
    public struct Hash16
    {

        private readonly byte[] hash;

        private Hash16(byte[] hash) { this.hash = hash; }


        public static Hash16 ComputeHashFromString(string data)
        {
            byte[] bytes;
            using (HashAlgorithm HA = MD5.Create())
            {
                bytes = HA.ComputeHash(Encoding.UTF8.GetBytes(data));
            }

            return new Hash16(bytes);
        }

        public static Hash16 ReadString(string hash)
        {
            byte[] bytes = System.Convert.FromHexString(hash);
            return new Hash16(bytes);
        }

        public override string ToString()
        {
           return System.Convert.ToHexString(hash);
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Hash16 other = (Hash16)obj;
            for (int i = 0; i<16 ; i++)
            {
                if(this.hash[i] != other.hash[i]) return false;
            }
            return true;
        }

        public static bool operator==(Hash16 lhs, Hash16 rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Hash16 lhs, Hash16 rhs)
        {
            return !lhs.Equals(rhs);
        }

        public override int GetHashCode()
        {
            return BitConverter.ToInt32(hash, 0);
        }
    }
}
