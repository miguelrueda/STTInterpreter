using System;
using System.Text;

namespace FilesInterpretations
{

    public class ServiceInfo
    {
        public string name;
        public bool isAllocated;
        public bool isActivated;
        public bool isMandatory;

        /// <summary>
        /// Base Constructor
        /// </summary>
        /// <param name="p_name">The name of the service</param>
        /// <param name="p_isAllocated">True if the service is allocated</param>
        /// <param name="p_isActivated">True if the service is activated</param>
        /// <param name="p_isMandatory">True if the service is mandatory</param>
        public ServiceInfo(string p_name, bool p_isAllocated, bool p_isActivated, bool p_isMandatory) 
        {
            name = p_name;
            isAllocated = p_isAllocated;
            isActivated = p_isActivated;
            isMandatory = p_isMandatory;
        }

        /// <summary>
        /// Bool array representation of isAllocated and isActivated
        /// position 0 represents the lsb
        /// position 1 represents the msb
        /// </summary>
        /// <returns>An array with the service representation</returns>
        public bool[] ToBoolArray()
        {
            bool[] array = new bool[2];
            array[0] = isAllocated;
            array[1] = isActivated;
            return array;
        }

        /// <summary>
        /// String representation
        /// </summary>
        /// <returns>A string with the current params</returns>
        public override string ToString()
        {
            return string.Format("(name = {0}, isAllocated = {1}, isActivated = {2}, isMandatory = {3})", name, isAllocated, isActivated, isMandatory);
        }

    }

    public interface IInterpreter
    {
        ServiceInfo[] Parse(byte[] bytes);
        byte[] ToBytes(ServiceInfo[] services);
    }

    public class STTInterpreter : IInterpreter
    {
        /// <summary>
        /// Parses a byte array to an Service Info Array
        /// </summary>
        /// <param name="bytes">An array of bytes to be represented as service info objects</param>
        /// <returns>An array of ServiceInfo</returns>
        public ServiceInfo[] Parse(byte[] bytes)
        {
            // Validate if the array is null or empty
            if (bytes.Length == 0)
            {
                Console.WriteLine("Input arrray is empty");
                return null;
            }

            // Validate the condition that X bytes >= 2
            if (bytes.Length < 2)
            {
                Console.WriteLine("Cannot operate with 2 or less bytes");
                return null;
            }

            // The length of the array  is calculated by the number of bytes on the input
            ServiceInfo[] servicesArr = new ServiceInfo[bytes.Length * 4];
            // An index to keep track of the output array
            int index = 0;
            bool mandatory = true;
            // Iterate through the byte array, for each byte we are extracting 4 ServiceInfo objects
            // hence, the +4 to the output index
            foreach (byte b in bytes)
            {
                // Only the first 2 services are marked as mandatory
                if (index > 7)
                {
                    mandatory = false;
                }
                // get the representation of the services from the current byte
                addServices(b, index, servicesArr, mandatory);
                index += 4;
            }

            return servicesArr;
        }

        /// <summary>
        /// Get the bits from the byte in order to create 4 services from each byte
        /// </summary>
        /// <param name="inputByte">The byte to read</param>
        /// <param name="index">The index to start on the array</param>
        /// <param name="services">The output array</param>
        /// <param name="isMandatory">Whether this byte is mandatory or not</param>
        private void addServices(byte inputByte, int index, ServiceInfo[] services, bool isMandatory)
        {
            // Start from the lsb of the byte, in this case we assume is position 7
            int position = 7;
            while (position > 0)
            {
                // Get the lsb from the pair
                bool isAllocated = GetBit(inputByte, position);
                // Get the msb from the pair
                bool isActivated = GetBit(inputByte, position - 1);
                // Create the serviceInfo object and add it to the output array
                ServiceInfo serviceInfo = new ServiceInfo(string.Format("Service-{0}", index), isAllocated, isActivated, isMandatory);
                services[index] = serviceInfo;
                // Increment the output index
                index++;
                // Decrement the position by 2
                position -= 2;
            }
        }

        /// <summary>
        /// Returns the Bit of the given position
        /// </summary>
        /// <param name="b">Input byte to retrieve the bit</param>
        /// <param name="index">The index of the desired bit</param>
        /// <returns></returns>
        private bool GetBit(byte b, int index)
        {
            // Validate that the index is in range of size of the byte
            if (index < 0 || index > 7)
            {
                throw new ArgumentOutOfRangeException();
            }
            // Calculate the number of left shifts
            int shift = 7 - index;

            // Create a mask that will help to retrieve the bit
            byte bitMask = (byte)(1 << shift);
            // Operate with AND to retrieve the bit at index position
            byte masked = (byte)(b & bitMask);

            // If the bit is already true it will return true
            return masked != 0;
        }

        /// <summary>
        /// Parses a ServiceInfo array to a byte array
        /// </summary>
        /// <param name="services">An array of ServiceInfo to be representes as bytes</param>
        /// <returns>An array of bytes</returns>
        public byte[] ToBytes(ServiceInfo[] services)
        {
            // Validate if the array is null or empty
            if (services.Length == 0)
            {
                return null;
            }
            // Validate that size is at least 8 to generate the condition X >= 2
            if (services.Length < 8)
            {
                return null;
            }
            // Get the length of the input array
            int servicesLength = services.Length;
            // The output array will be of size input / 4 - a byte is formed by 4 ServiceInfo objects
            byte[] parsed = new byte[servicesLength / 4];
            // An index to track the output array
            int index = 0;
            // We will iterate all the array but take 4 objects at each time
            for (int i = 0; i < servicesLength; i += 4)
            {
                // Form a byte by getting 4 ServiceInfo objects
                addBytes(GetSubArray(services, i, i + 3), parsed, index);
                index += 1;
            }
            return parsed;
        }
        
        /// <summary>
        /// This method adds the Service Info objects to the byte array
        /// </summary>
        /// <param name="array">The input array with ServiceInfo objects</param>
        /// <param name="result">Reference for byte array output</param>
        /// <param name="b_index">The index of the array</param>
        public void addBytes(ServiceInfo [] array, byte [] result, int b_index)
        {
            // Create an array of boolean with the size of the byte
            bool[] boolByte = new bool[8];
            // Start index
            int auxIndex = 7;
            // Iterate the array of service info objects
            for (int i = 0; i < array.Length; i++)
            {
                // Extract the bool array representation of the service and append it to the bool array
                bool [] repr = array[i].ToBoolArray();
                boolByte[auxIndex] = repr[0];
                boolByte[auxIndex - 1] = repr[1];
                auxIndex -= 2;
            }
            // Create a byte from the bool array and append it
            result[b_index] = BoolArrayToByte(boolByte);
        }

        /// <summary>
        /// A helper that will help to take 4 elements at a time from an array
        /// </summary>
        /// <param name="array">The input array</param>
        /// <param name="start">The start of the subarray</param>
        /// <param name="end">The end of the subarray</param>
        /// <returns></returns>
        private ServiceInfo[] GetSubArray(ServiceInfo[] array, int start, int end)
        {
            // Initialize the array
            ServiceInfo[] result = new ServiceInfo[end - start + 1];
            int index = 0;
            // Just add the elements
            for (int i = start; i <= end; i++)
            {
                result[index] = array[i];
                index++;
            }
            return result;
        }

        /// <summary>
        /// Transforms a bool array to a byte
        /// </summary>
        /// <param name="array">the input bool array</param>
        /// <returns>a byte from the bool array</returns>
        private byte BoolArrayToByte(bool[] array)
        {
            byte b = 0;
            // Calculate the index to start
            int index = 8 - array.Length;
            // Iterate through the array
            foreach (bool _bool in array)
            {
                // If the bool is true, shift it to the corresponding index
                if (_bool)
                {
                    // If it is true just an OR operation is enough
                    b |= (byte)(1 << (7 - index));
                }
                index++;
            }
            return b;
        }

    }
    class Program
    {
        static void Main(string[] args)
        {
            // Test code
            Console.WriteLine("Executing");

            // 11001001
            // Output from string
            string strInput = "11001001 11000000 01111111 01100001 00111010 00110000 11011111 11101011 01100000 00010111";
            Console.WriteLine(string.Format("Input {0}", strInput));
            // Convert to string array
            string[] words = strInput.Split(" ");
            foreach (string word in words)
            {
                Console.WriteLine(word);
            }
            // Convert to byte array
            byte[] bytesArr = new byte[words.Length];
            for (int i = 0; i < words.Length; i++)
            {
                bytesArr[i] = Convert.ToByte(words[i], 2);
            }

            // Debug the content
            Console.WriteLine("Byte Array from string");
            foreach (byte b in bytesArr)
            {
                Console.WriteLine(Convert.ToString(b, 2).PadLeft(8, '0'));
            }

            // Instance of the interpreter
            STTInterpreter sttInterpreter = new STTInterpreter();
            ServiceInfo[] parsed = sttInterpreter.Parse(bytesArr);
            foreach (ServiceInfo serviceinfo in parsed)
            {
                Console.WriteLine(serviceinfo);
            }
            // Back to the byte array
            byte[] original = sttInterpreter.ToBytes(parsed);
            foreach(byte b in original)
            {
                Console.WriteLine(Convert.ToString(b, 2).PadLeft(8, '0'));
            }
        }
    }
}
