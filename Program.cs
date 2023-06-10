using Google.Cloud.TextToSpeech.V1;
using System.Media;
using Spectre.Console;

Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "auth.json");
var client = TextToSpeechClient.Create();
string[] letters = { "AAA", "W", "T", "OOO" };

SynthesisInput CreateChoiceVoice(string letter)
{
  return new SynthesisInput { Text = $"Agora digite a letra: {letter}" };
}

var voiceParams = new VoiceSelectionParams
{
  LanguageCode = "pt-Br",
  SsmlGender = SsmlVoiceGender.Male
};
var voiceConfig = new AudioConfig { AudioEncoding = AudioEncoding.Linear16 };

// foreach (var item in letters)
// {
//   CreateWav(item, voiceParams, voiceConfig, item);
// }

void CreateWav(string letter, VoiceSelectionParams param, AudioConfig config, string name)
{
  var response = client.SynthesizeSpeech(new SynthesizeSpeechRequest
  {
    Input = CreateChoiceVoice(letter),
    Voice = param,
    AudioConfig = config
  });

  using (Stream output = File.Create($"{name}.wav")) { response.AudioContent.WriteTo(output); }
}

var next = true;
var choiced = letters[Random.Shared.Next(0, letters.Length)];
string last = null;

while (true)
{
  Console.Clear();

  if (next)
  {
    var choiceAgain = true;
    while (choiceAgain)
    {
      choiced = letters[Random.Shared.Next(0, letters.Length)];
      choiceAgain = last == choiced;
    }

    if (string.IsNullOrWhiteSpace(last))
      last = choiced;
  }

  SoundPlayer player = new($"{choiced}.wav");
  AnsiConsole.Write(new FigletText(choiced[0].ToString()));
  AnsiConsole.WriteLine();
  player.Play();
  player.Dispose();

  await ExecuteChoice(player);

  Thread.Sleep(TimeSpan.FromSeconds(5));
}

Task<bool> ExecuteChoice(SoundPlayer player)
{
  return Task.Factory.StartNew(() =>
  {
    var typed = Console.ReadKey();

    AnsiConsole.WriteLine();

    if (typed.KeyChar.ToString().ToUpper() == choiced[0].ToString())
    {
      player = new SoundPlayer("congratulations.wav");
      AnsiConsole.WriteLine();
      AnsiConsole.Write(
        new FigletText("\\o/")
      );
      AnsiConsole.Write(
        new FigletText("    |")
      );
      AnsiConsole.Write(
        new FigletText("  /\\")
      );

      next = true;
    }
    else
    {
      player = new SoundPlayer("error.wav");
      AnsiConsole.Write(
        new FigletText(":(")
      );

      next = false;
    }

    player.Play();
    player.Dispose();

    return next;
  });
}