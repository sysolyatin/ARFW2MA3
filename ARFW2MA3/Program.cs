using System.Net;
using FastOSC;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;

namespace ARFW2MA3
{
    class Program
    {
        private static InputDevice? _inputDevice;
        private static readonly OSCSender Osc = new();

        private static readonly Dictionary<int, int> MaKeys = new()
        {
            { 16, 401 }, { 17, 402 }, { 18, 403 }, { 19, 404 }, { 20, 405 },
            { 21, 406 }, { 22, 407 }, { 23, 408 }, { 24, 409 }, { 25, 410 },
            { 26, 411 }, { 27, 412 }, { 28, 413 }, { 29, 414 }, { 30, 415 },
            
            { 31, 301 }, { 32, 302 }, { 33, 303 }, { 34, 304 }, { 35, 305 },
            { 36, 306 }, { 37, 307 }, { 38, 308 }, { 39, 309 }, { 40, 310 },
            { 41, 311 }, { 42, 312 }, { 43, 313 }, { 44, 314 }, { 45, 315 },
            
            { 46, 201 }, { 47, 202 }, { 48, 203 }, { 49, 204 }, { 50, 205 },
            { 51, 206 }, { 52, 207 }, { 53, 208 }, { 54, 209 }, { 55, 210 },
            { 56, 211 }, { 57, 212 }, { 58, 213 }, { 59, 214 }, { 60, 215 },
            
            { 61, 101 }, { 62, 102 }, { 63, 103 }, { 64, 104 }, { 65, 105 },
            { 66, 106 }, { 67, 107 }, { 68, 108 }, { 69, 109 }, { 70, 110 },
            { 71, 111 }, { 72, 112 }, { 73, 113 }, { 74, 114 }, { 75, 115 },
            
            // X Keys
            { 76, 291 }, { 77, 292 }, { 78, 293 }, { 79, 294 }, { 80, 295 }, { 81, 296 }, { 82, 297 }, { 83, 298 },
            { 91, 191 }, { 92, 192 }, { 93, 193 }, { 94, 194 }, { 95, 195 }, { 96, 196 }, { 97, 197 }, { 98, 198 }
        };
        

        static async Task Main(string[] args)
        {
            Console.WriteLine(@"                         _           _____                            ");
            Console.WriteLine(@"     /\                 | |         |  __ \                           ");
            Console.WriteLine(@"    /  \     _ __     __| |  _   _  | |__) |   ___     ___            ");
            Console.WriteLine(@"   / /\ \   | '_ \   / _` | | | | | |  _  /   / _ \   / __|           ");
            Console.WriteLine(@"  / ____ \  | | | | | (_| | | |_| | | | \ \  | (_) | | (__            ");
            Console.WriteLine(@" /_/    \_\ |_| |_|  \__,_|  \__, | |_|  \_\  \___/   \___|           ");
            Console.WriteLine(@"                              __/ |                                   ");
            Console.WriteLine(@"                             |___/                                    ");
            Console.WriteLine(@"  ______  __          __    _               __  __              ____  ");
            Console.WriteLine(@" |  ____| \ \        / /   | |             |  \/  |     /\     |___ \ ");
            Console.WriteLine(@" | |__     \ \  /\  / /    | |_    ___     | \  / |    /  \      __) |");
            Console.WriteLine(@" |  __|     \ \/  \/ /     | __|  / _ \    | |\/| |   / /\ \    |__ < ");
            Console.WriteLine(@" | |         \  /\  /      | |_  | (_) |   | |  | |  / ____ \   ___) |");
            Console.WriteLine(@" |_|          \/  \/        \__|  \___/    |_|  |_| /_/    \_\ |____/ ");
            
            await Osc.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 8000));
            
            SetupMidi();

            Console.WriteLine("");

            while (true)
            {
                Thread.Sleep(100);
            }
        }
        


        static void SetupMidi()
        {
            var inputMidiDevices = InputDevice.GetAll().ToList();
            var device = inputMidiDevices
                .FirstOrDefault(d => 
                    string.Equals(d.Name, "Fader Wing MIDI", StringComparison.CurrentCultureIgnoreCase));

            if (device == null)
            {
                Console.WriteLine("\nMIDI-устройство Fader Wing MIDI не найдено.");
                Console.WriteLine("Нажмите любую клавишу для выхода...");
                Console.ReadKey();
                Environment.Exit(1);
            }
            
            try
            {
                Console.WriteLine($"\nПодключение к: {device.Name}");
                
                _inputDevice = device;
                _inputDevice.EventReceived += OnMidiEventReceived;
                _inputDevice.ErrorOccurred += OnMidiError;
                _inputDevice.StartEventsListening();
                
                Console.WriteLine("Устройство успешно подключено!\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nОшибка подключения к MIDI устройству: {ex.Message}");
                Console.WriteLine("Нажмите любую клавишу для выхода...");
                Console.ReadKey();
                Environment.Exit(1);
            }
        }

        static void OnMidiEventReceived(object sender, MidiEventReceivedEventArgs e)
        {
            var midiEvent = e.Event;
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var originalColor = Console.ForegroundColor;

            try
            {
                string output = $"[{timestamp}] ";

                switch (midiEvent)
                {
                    case NoteOnEvent noteOn:
                        var noteOnNumber = (int)noteOn.NoteNumber;
                        if (MaKeys.TryGetValue(noteOnNumber, out var keyUpNumber))
                        {
                            Osc.Send(new OSCMessage($"/Key{keyUpNumber}", 100));
                            output += $"Note On | Нота: {noteOnNumber} => Button {keyUpNumber}";
                        }
                        else
                        {
                            output += $"Note On | Нота: {noteOnNumber}";
                        }
                        
                        Console.ForegroundColor = noteOn.Velocity > 0 ? ConsoleColor.Green : ConsoleColor.DarkGreen;
                        break;

                    case NoteOffEvent noteOff:
                        var noteOffNumber = (int)noteOff.NoteNumber;
                        if (MaKeys.TryGetValue(noteOffNumber, out var keyDownNumber))
                        {
                            Osc.Send(new OSCMessage($"/Key{keyDownNumber}", 0));
                            output += $"Note Off | Нота: {noteOffNumber} => Button {keyDownNumber}";
                        }
                        else
                        {
                            output += $"Note Off | Нота: {noteOffNumber}";
                        }
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        break;

                    case ControlChangeEvent controlChange:
                        var controlNumber = (int)controlChange.ControlNumber;
                        var midiValue = (int)controlChange.ControlValue;
                        var maFaderExecNumber = controlNumber + 200;
                        var oscValue = midiValue * 100 / 127;
                        Osc.Send(new OSCMessage($"/Fader{maFaderExecNumber}", oscValue));
                        output += $"MIDI Control: {controlNumber} | Значение: {midiValue} => Executor {maFaderExecNumber}";
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        break;
                }

                Console.WriteLine(output);
                Console.ForegroundColor = originalColor;
            }
            catch (Exception ex)
            {
                if (ex.Message == "Please call ConnectAsync first")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[{timestamp}] Похоже, не произошло подключение по OSC к МА3");
                    Console.WriteLine($"[{timestamp}] Нужно создать вход без префикса на порту 8000");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[{timestamp}] Ошибка обработки события: {ex.Message}");
                }
                Console.ForegroundColor = originalColor;
            }
        }

        static void OnMidiError(object sender, ErrorOccurredEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ОШИБКА] {e.Exception.Message}");
            Console.ResetColor();
        }
    }
}