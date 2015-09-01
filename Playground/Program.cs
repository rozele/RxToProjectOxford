//
// Copyright Microsoft Corporation.  All rights reserved.
// ericroz - Sept 2015
//

using MicrosoftProjectOxford;
using RxToProjectOxford;
using System;
using System.Configuration;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Playground
{
    class Program
    {
        static void Main()
        {
            using (var client = SpeechRecognitionServiceFactory.CreateDataClient(SpeechRecognitionMode.LongDictation, "en-US", ConfigurationManager.AppSettings["primaryKey"]))
            using (EventBasedSample(client)) /* uncomment for event-driven */
            //using (ReactiveSample(client)) /* uncomment for Reactive */
            {
                Console.WriteLine("Press any key to start listening...");
                Console.ReadKey();
                Console.Write("Starting microphone... ");
                client.AudioStart();
                Console.WriteLine("Done.");
                Console.WriteLine("Press any key to stop listening...");
                Console.ReadKey();
                client.AudioStop();
                Console.WriteLine("Microphone stopped.");
            }
        }

        static IDisposable EventBasedSample(DataRecognitionClient client)
        {
            var count = 0;

            EventHandler<SpeechErrorEventArgs> errorHandler = (sender, args) =>
            {
                Console.Error.WriteLine("Failed with code '{0}' and text '{1}'.", args.SpeechErrorCode, args.SpeechErrorText);
            };

            EventHandler<PartialSpeechResponseEventArgs> partialHandler = (sender, args) =>
            {
                Console.CursorLeft = 0;
                var prefix = (count == 0) ? "Title" : "Sentence " + count;
                Console.Write("{0}: {1}", prefix, args.PartialResult);
            };

            EventHandler<SpeechResponseEventArgs> responseHandler = (sender, args) =>
            {
                if (args.PhraseResponse.RecognitionStatus == RecognitionStatus.RecognitionSuccess)
                {
                    var result = args.PhraseResponse.Results.First().DisplayText;
                    Console.CursorLeft = 0;
                    var prefix = (count == 0) ? "Title" : "Sentence " + count;
                    Console.WriteLine("{0}: {1}", prefix, result);
                    count++;
                }
            };

            client.OnConversationError += errorHandler;
            client.OnPartialResponseReceived += partialHandler;
            client.OnResponseReceived += responseHandler;

            return Disposable.Create(() =>
            {
                client.OnConversationError -= errorHandler;
                client.OnPartialResponseReceived -= partialHandler;
                client.OnResponseReceived -= responseHandler;
            });
        }

        static IDisposable ReactiveSample(DataRecognitionClient client)
        {
            var disposable = new CompositeDisposable();
            var counter = Enumerable.Range(0, int.MaxValue);
            var sentenceSubscriptions = client.GetResponseObservable()
                .Zip(counter, (observable, count) => new { observable, count })
                .Subscribe(
                    x => disposable.Add(x.observable.Subscribe(
                        phrases =>
                        {
                            Console.CursorLeft = 0;
                            var firstPhrase = phrases.First();
                            var prefix = x.count == 0 ? "Title" : "Sentence " + x.count;
                            Console.Write("{0}: {1}", prefix, firstPhrase.DisplayText ?? firstPhrase.LexicalForm);
                        },
                        ex => Console.Error.WriteLine(ex),
                        () => Console.WriteLine())));

            disposable.Add(sentenceSubscriptions);

            return disposable;
        }
    }
}
