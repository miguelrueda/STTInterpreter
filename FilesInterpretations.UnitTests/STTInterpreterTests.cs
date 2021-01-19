using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FilesInterpretations.UnitTests
{
    [TestClass]
    public class STTInterpreterTests
    {
        [TestMethod]
        public void Parsed_EmptyInput_NullOutput()
        {
            // Arrange
            STTInterpreter stt = new STTInterpreter();
            byte[] arr = new byte[0];

            // Act
            ServiceInfo[] array = stt.Parse(arr);

            // Assert
            Assert.AreEqual(null, array);
        }

        [TestMethod]
        public void Parsed_1ByteInput_NullOutput()
        {
            // Arrange
            STTInterpreter stt = new STTInterpreter();
            byte[] arr = new byte[1] { 0 };

            // Act
            ServiceInfo[] array = stt.Parse(arr);

            // Assert
            Assert.AreEqual(null, array);
        }

        [TestMethod]
        public void Parsed_2BytesInput_8ServicesOutput()
        {
            // Arrange
            STTInterpreter stt = new STTInterpreter();
            byte[] arr = new byte[2] { 100, 120 };

            // Act
            ServiceInfo[] array = stt.Parse(arr);

            // Assert
            Assert.AreEqual(8, array.Length);
        }

        [TestMethod]
        public void Parsed_10BytesInput_40ServicesOutput()
        {
            // Arrange
            STTInterpreter stt = new STTInterpreter();
            byte[] arr = new byte[10] { 201, 192, 127, 97, 58, 48, 223, 235, 96, 23 };

            // Act
            ServiceInfo[] array = stt.Parse(arr);

            // Assert
            Assert.AreEqual(40, array.Length);
        }

        [TestMethod]
        public void Parsed_2BytesInput_8ServicesMandatory()
        {
            // Arrange
            STTInterpreter stt = new STTInterpreter();
            byte[] arr = new byte[2] { 201, 192 };

            // Act
            ServiceInfo[] array = stt.Parse(arr);

            // Assert
            foreach (ServiceInfo service in array)
            {
                Assert.IsTrue(service.isMandatory);
            }
        }

        [TestMethod]
        public void Parsed_BytesInputAllOnes_AllServicesAllocatedAndActivated()
        {
            // Arrange
            STTInterpreter stt = new STTInterpreter();
            byte[] arr = new byte[5] { 255, 255, 255, 255, 255 };

            // Act
            ServiceInfo[] array = stt.Parse(arr);

            // Assert
            foreach (ServiceInfo service in array)
            {
                Assert.IsTrue(service.isAllocated);
                Assert.IsTrue(service.isActivated);
            }
        }

        [TestMethod]
        public void Parsed_BytesInputAllZeros_ServicesNotAllocatedNorActivated()
        {
            // Arrange
            STTInterpreter stt = new STTInterpreter();
            byte[] arr = new byte[5] { 0, 0, 0, 0, 0 };

            // Act
            ServiceInfo[] array = stt.Parse(arr);

            // Assert
            foreach (ServiceInfo service in array)
            {
                Assert.IsFalse(service.isAllocated);
                Assert.IsFalse(service.isActivated);
            }
        }

        [TestMethod]
        public void ToBytes_EmptyInput_NullOutput()
        {
            // Arrange
            STTInterpreter stt = new STTInterpreter();
            ServiceInfo[] array = new ServiceInfo[0];

            // Act
            byte[] output = stt.ToBytes(array);

            // Assert
            Assert.AreEqual(null, output);
        }

        [TestMethod]
        public void ToBytes_4ServicesInput_NullOutput()
        {
            // Arrange
            STTInterpreter stt = new STTInterpreter();
            ServiceInfo[] array = generateServiceInfoArray(4, true, true);

            // Act
            byte[] output = stt.ToBytes(array);

            // Assert
            Assert.AreEqual(null, output);
        }

        [TestMethod]
        public void ToBytes_8ServicesInput_2BytesOutput()
        {
            // Arrange
            STTInterpreter stt = new STTInterpreter();
            ServiceInfo[] array = generateServiceInfoArray(8, true, true);

            // Act
            byte[] output = stt.ToBytes(array);

            // Assert
            Assert.AreEqual(2, output.Length);
        }

        [TestMethod]
        public void ToBytes_40ServicesInput_10BytesOutput() {
            // Arrange
            STTInterpreter stt = new STTInterpreter();
            ServiceInfo[] array = generateServiceInfoArray(40, true, true);

            // Act
            byte[] output = stt.ToBytes(array);

            // Assert
            Assert.AreEqual(10, output.Length);
        }

        [TestMethod]
        public void ToBytes_ServicesAllocatednActivated_AllBytesOne() {
            // Arrange
            STTInterpreter stt = new STTInterpreter();
            ServiceInfo[] array = generateServiceInfoArray(40, true, true);

            // Act
            byte[] output = stt.ToBytes(array);

            // Assert
            foreach(byte b in output)
            {
                Assert.AreEqual(255, b);
            }
        }

        [TestMethod]
        public void ToBytes_ServicesNotAllocatedNorActivated_AllBytesZero() {
            // Arrange
            STTInterpreter stt = new STTInterpreter();
            ServiceInfo[] array = generateServiceInfoArray(40, false, false);

            // Act
            byte[] output = stt.ToBytes(array);

            // Assert
            foreach (byte b in output)
            {
                Assert.AreEqual(0, b);
            }
        }

        #region Helper methods
        private ServiceInfo[] generateServiceInfoArray(int size, bool allocated, bool activated)
        {
            ServiceInfo[] services = new ServiceInfo[size];
            for (int i = 0; i < size; i++)
            {
                ServiceInfo service = new ServiceInfo(string.Format("Service-{0}", i), allocated, activated, false);
                services[i] = service;
            }
            return services;
        }
        #endregion
    }

}
