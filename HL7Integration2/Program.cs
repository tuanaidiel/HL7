using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        string sampleMessage = "MSH|^~\\&|SENDING_APP|SENDING_FAC|RECEIVING_APP|RECEIVING_FAC|20240122|SECURITY|ADT^A03|MSG00001|P|2.3\n" +
                             "PID|||12345^^^MRN||DOE^JOHN^^^^||19700101|M|||123 MAIN ST^^ANYTOWN^ST^12345||555-555-5555|||S||MRN12345|123-45-6789|||||||||||\n" +
                             "PV1||I|2000^2012^01||||004777^GOOD^PATRICIA^^^^^^|||MED||||A0||004777^GOOD^PATRICIA^^^^^^|S|1234567890|B6|||||||||||||||||||||||||20240122";

        var handler = new HL7MessageHandler();
        handler.ProcessMessage(sampleMessage);
    }
}

public class HL7Message
{
    private readonly string[] segments;
    
    public HL7Message(string rawMessage)
    {
        segments = rawMessage.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
    }

    private string GetSegmentValue(string segmentName, int fieldIndex)
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
            return fullId.Split('^')[0]; // Get just the ID number
        }
    }

    public string PatientName
    {
        get
        {
            string fullName = GetSegmentValue("PID", 5);
            var nameParts = fullName.Split('^');
            if (nameParts.Length >= 2)
                return $"{nameParts[0]}, {nameParts[1]}"; // Format as "LastName, FirstName"
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
            Console.WriteLine($"Received message type: {message.MessageType}");

            switch (message.MessageType)
            {
                case "ADT^A01":
                    HandleAdmission(message);
                    break;
                case "ADT^A03":
                    HandleDischarge(message);
                    break;
                default:
                    Console.WriteLine($"Unsupported message type: {message.MessageType}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing HL7 message: {ex.Message}");
        }
    }

    private void HandleAdmission(HL7Message message)
    {
        Console.WriteLine($"Processing admission for patient:");
        Console.WriteLine($"Patient ID: {message.PatientId}");
        Console.WriteLine($"Patient Name: {message.PatientName}");
    }

    private void HandleDischarge(HL7Message message)
    {
        Console.WriteLine($"Processing discharge for patient:");
        Console.WriteLine($"Patient ID: {message.PatientId}");
        Console.WriteLine($"Patient Name: {message.PatientName}");
    }
}

