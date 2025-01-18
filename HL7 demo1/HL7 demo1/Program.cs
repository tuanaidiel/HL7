// First, make sure to install these NuGet packages:
// dotnet add package NHapi.Base
// dotnet add package NHapi.Model.V231

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using NHapi.Base.Parser;
using NHapi.Base.Model;
using NHapi.Model.V231.Message;
using NHapi.Model.V231.Segment;
using NHapi.Model.V231.Datatype;
using NHapi.Base;

namespace HL7Integration
{
    public class Program
    {
        static void Main(string[] args)
        {
            var integration = new HL7Integration();

            // Create and send a sample message
            var hl7Message = integration.CreateSampleADT_A01();
            Console.WriteLine("Created HL7 Message:");
            Console.WriteLine(integration.SerializeMessage(hl7Message));

            // Parse a received message
            string receivedMessage = @"MSH|^~\&|SENDING_APP|SENDING_FAC|RECEIVING_APP|RECEIVING_FAC|20240118|SECURITY|ADT^A01|MSG00001|P|2.3
PID|||12345^^^MRN||DOE^JOHN^^^^||19800101|M|||123 MAIN ST^^ANYTOWN^NY^12345||555-555-5555|||S||MRN12345|123-45-6789|||||||||||";

            integration.ParseAndProcessMessage(receivedMessage);
        }
    }

    public class HL7Integration
    {
        private PipeParser parser;
        private int messageControlId = 1;

        public HL7Integration()
        {
            parser = new PipeParser();
        }

        public ADT_A01 CreateSampleADT_A01()
        {
            ADT_A01 adtMessage = new ADT_A01();

            // MSH segment
            adtMessage.MSH.FieldSeparator.Value = "|";
            adtMessage.MSH.EncodingCharacters.Value = "^~\\&";
            adtMessage.MSH.SendingApplication.NamespaceID.Value = "SENDING_APP";
            adtMessage.MSH.SendingFacility.NamespaceID.Value = "SENDING_FAC";
            adtMessage.MSH.ReceivingApplication.NamespaceID.Value = "RECEIVING_APP";
            adtMessage.MSH.ReceivingFacility.NamespaceID.Value = "RECEIVING_FAC";
            adtMessage.MSH.DateTimeOfMessage.TimeOfAnEvent.Value = DateTime.Now.ToString("yyyyMMddHHmmss");
            adtMessage.MSH.MessageType.MessageType.Value = "ADT";
            adtMessage.MSH.MessageType.TriggerEvent.Value = "A01";
            adtMessage.MSH.MessageControlID.Value = GetNextControlId();
            adtMessage.MSH.ProcessingID.ProcessingID.Value = "P";
            

            // EVN segment
            adtMessage.EVN.EventTypeCode.Value = "A01";
            adtMessage.EVN.RecordedDateTime.TimeOfAnEvent.Value = DateTime.Now.ToString("yyyyMMddHHmmss");

            // PID segment
            adtMessage.PID.SetIDPID.Value = "1";

            // Set Patient ID (CX)
            var patientId = adtMessage.PID.GetPatientIdentifierList(0);
            patientId.ID.Value = "12345";
            patientId.AssigningAuthority.NamespaceID.Value = "MRN";

           

          

            return adtMessage;
        }

        public void ParseAndProcessMessage(string messageText)
        {
            try
            {
                IMessage message = parser.Parse(messageText);

                if (message is ADT_A01 adtMessage)
                {
                    Console.WriteLine("\nProcessed Message Details:");
                    Console.WriteLine($"Message Type: {adtMessage.MSH.MessageType.MessageType.Value}");
                    Console.WriteLine($"Trigger Event: {adtMessage.MSH.MessageType.TriggerEvent.Value}");
                    Console.WriteLine($"Message Control ID: {adtMessage.MSH.MessageControlID.Value}");
                    Console.WriteLine($"Patient ID: {adtMessage.PID.GetPatientIdentifierList(0).ID.Value}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing message: {ex.Message}");
            }
        }

        public string SerializeMessage(IMessage message)
        {
            return parser.Encode(message);
        }

        private string GetNextControlId()
        {
            return (messageControlId++).ToString("D10");
        }
    }

    public class HL7Listener
    {
        private TcpListener listener;
        private readonly int port;
        private readonly HL7Integration hl7Integration;
        private bool isRunning;

        public HL7Listener(int port)
        {
            this.port = port;
            this.hl7Integration = new HL7Integration();
        }

        public void Start()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                isRunning = true;

                Console.WriteLine($"HL7 Listener started on port {port}");

                while (isRunning)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    HandleClient(client);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in listener: {ex.Message}");
            }
        }

        private void HandleClient(TcpClient client)
        {
            try
            {
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] buffer = new byte[4096];
                    StringBuilder messageBuilder = new StringBuilder();
                    int bytesRead;

                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        string chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        messageBuilder.Append(chunk);

                        if (chunk.EndsWith("\r\n"))
                            break;
                    }

                    string message = messageBuilder.ToString();

                    hl7Integration.ParseAndProcessMessage(message);

                    string ack = CreateAcknowledgment(message);
                    byte[] responseBytes = Encoding.UTF8.GetBytes(ack);
                    stream.Write(responseBytes, 0, responseBytes.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }

        private string CreateAcknowledgment(string originalMessage)
        {
            return "MSH|^~\\&|RECEIVING_APP|RECEIVING_FAC|SENDING_APP|SENDING_FAC|" +
                   DateTime.Now.ToString("yyyyMMddHHmmss") + "||ACK^A01|ACK" +
                   DateTime.Now.Ticks + "|P|2.3.1\r" +
                   "MSA|AA|" + DateTime.Now.Ticks + "|Message received successfully\r\n";
        }

        public void Stop()
        {
            isRunning = false;
            listener?.Stop();
        }
    }
}