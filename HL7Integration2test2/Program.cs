using System;

class Program
{
    static void Main(string[] args)
    {
        string HL7Message = "MSH|monitor|hospial|central|hospital|ORU^O01|P|2.5\n" +
                            "PID|Bakar^Abu|M|20010306\n" +
                            "OBX|100|mg/dL|N";

        string [] segments = HL7Message.Split('\n');
        string [] mshSegment = segments[0].Split('|');
        string [] pidSegment = segments[1].Split('|');
        string [] obxSegment = segments[2].Split('|');

        string [] nameParts = pidSegment[1].Split('^');
        string patientName = $"{nameParts[1]} {nameParts[0]}";

        System.Console.WriteLine($"Message type: {mshSegment[5]}");
        System.Console.WriteLine($"Patient name: {patientName}");
        System.Console.WriteLine($"Status: {obxSegment[3]}");
    }
}