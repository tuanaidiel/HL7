using System;

class Program
{
    static void Main(string[] args)
    {
        string sampleMessage = "MSH|monitor|hospial|central|hospital|ORU^O01|P|2.5\n" +
                                "PID|Bakar^Abu^^^|M|20010306\n" +
                                "OBX|100|mg/dL|N";
        
        string[] segments = sampleMessage.Split('\n');

        string[] mshSegment = null!;
        foreach (string segment in segments)
        {
            if(segment.StartsWith("MSH|"))
            {
                mshSegment = segment.Split('|'); 
            }
        }
        if (mshSegment == null)
        {
            System.Console.WriteLine("MSH segment not found!");
            return;
        }

        //2nd: string[] mshSegment = segment[0].Split('|');
        string[] pidSegment = segments[1].Split('|');
        string[] obxSegment = segments[2].Split('|');

        string[] namePart = pidSegment[1].Split('^');
        string patientName = $"{namePart[1]} {namePart[0]}";

        string status = obxSegment[3] == "N" ? "Normal" : "Abnormal";

        System.Console.WriteLine($"Message type: {mshSegment[5]}");
        System.Console.WriteLine($"Patient name: {patientName}");
        System.Console.WriteLine($"Status: {status}");
    }
}

//public class HL7MessageHandler
//{
//    public void ProcessMessage(string message)
//   {
//        string [] segments = message.Split('\n');
//        string [] mshSegment = segments[0].Split('|');
//        string [] pidSegment = segments[1].Split('|');
//        string [] obxSegment = segments[2].Split('|');

//        string [] nameParts = pidSegment[1].Split('^');
//        string patientName = $"{nameParts[1]} {nameParts[0]}";

//       string status = obxSegment[3] == "N" ? "Normal" : "Abnormal";

//        System.Console.WriteLine($"Message type: {mshSegment[5]}");
//        System.Console.WriteLine($"Patient name: {patientName}");
//        System.Console.WriteLine($"Status: {status}");
//    }
//}