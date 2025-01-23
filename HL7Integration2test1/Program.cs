using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        string sampleMessage = "MSH|^~\\&|SENDING_APP|SENDING_FAC|RECEIVING_APP|RECEIVING_FAC|20250123|SECURITY|ADT^A01|MSG00001|P|2.3\n" +
                                "PID|||12345^^^MRN||DOE^JOHN^^^^||20010306|M|||123 MAIN ST^ST^SEGAMAT||011-2779999|||S|MRN12345||||\n" +
                                "PV1||I|2000^2012^01||||004777^GOOD^PATRICIA^^^^^^|||MED||||A0||004777^GOOD^PATRICIA^^^|S|1234567890|B6|||20240122";

        var handler = new HL7MessageHandler();
        handler.ProcessMessage(sampleMessage);
    }
}

public class HL7Message
{
    private readonly string[] segments;

    public HL7Message(string rawMessage)
    {
        segments = rawMessage.Split(new[] { "\r", "\n"}, StringSplitOptions.RemoveEmptyEntries);
    }

    private string GetSegmentValue (string segmentName, int fieldIndex)
    {
        var segment = segments.FirstOrDefault(s => s.StartsWith(segmentName));
        if (segment == null) return string.Empty;

        var fields = segment.Split('|');
        return fieldIndex < fields.Length ? fields[fieldIndex] : string.Empty;
    }
    
    public string MessageType
    {
        get
        {
            var mshSegment = segments.FirstOrDefault(s => s.StartsWith("MSH"));
            if (mshSegment == null) return string.Empty;

            var fields = mshSegment.Split('|');
            return fields.Length > 9 ? fields[8] : string.Empty;
        }
    }

    public string PatientId
    {
        get
        {
            string fullId = GetSegmentValue("PID", 3);
            return fullId.Split('^')[0];
        }
    }

    public string PatientName
    {
        get
        {
            string fullName = GetSegmentValue("PID", 5);
            var nameParts = fullName.Split('^');
            if (nameParts.Length >= 2)
                return $"{nameParts[0]}, {nameParts[1]}";
            return nameParts[0];
        }
    }
}

public class HL7MessageHandler
{
    public void ProcessMessage(string rawHL7Message)
    {
        try
        {
            var message = new HL7Message(rawHL7Message);
            System.Console.WriteLine($"Received message type: {message.MessageType}");

            switch (message.MessageType)
            {
                case "ADT^A01":
                    HandleAdmission(message);
                    break;
                case "ADT^A03":
                    HandleDischarge(message);
                    break;
                default:
                    System.Console.WriteLine($"Unsupported message type: {message.MessageType}");
                    break;
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error processing HL7 message: {ex.Message}");
        }
    }
    private void HandleAdmission(HL7Message message)
    {
        System.Console.WriteLine($"Processing admission for patient:");
        System.Console.WriteLine($"Patient ID: {message.PatientId}");
        System.Console.WriteLine($"Patient Name: {message.PatientName}");
    }

    private void HandleDischarge(HL7Message message)
    {
        System.Console.WriteLine($"Processing discharge for patient:");
        System.Console.WriteLine($"Patient ID: {message.PatientId}");
        System.Console.WriteLine($"Patient Name: {message.PatientName}");
    }
}