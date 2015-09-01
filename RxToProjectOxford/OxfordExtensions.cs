//
// Copyright Microsoft Corporation.  All rights reserved.
// ericroz - Sept 2015
//

using MicrosoftProjectOxford;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace RxToProjectOxford
{
    /// <summary>
    /// Reactive extensions for Project Oxford.
    /// </summary>
    public static class OxfordReactiveExtensions
    {
        private static readonly IEnumerable<IRecognizedPhrase> s_thunk;

        /// <summary>
        /// Gets the response observable for the speech recognition client.
        /// </summary>
        /// <param name="client">The speech recognition client.</param>
        /// <returns>
        /// A response observable, which is a sequence of observables
        /// containing partial results, terminated by a final success and a
        /// completion event or an error event.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This scenario is excessively complex :-).")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "This scenario is excessively complex :-).")]
        public static IObservable<IObservable<IEnumerable<IRecognizedPhrase>>> GetResponseObservable(this DataRecognitionClient client)
        {
            var errorObservable = Observable.FromEventPattern<SpeechErrorEventArgs>(
                    h => client.OnConversationError += h,
                    h => client.OnConversationError -= h)
                .Select<EventPattern<SpeechErrorEventArgs>, IEnumerable<IRecognizedPhrase>>(
                    x => { throw new SpeechRecognitionException(x.EventArgs.SpeechErrorCode, x.EventArgs.SpeechErrorText); });

            var partialObservable = Observable.FromEventPattern<PartialSpeechResponseEventArgs>(
                    h => client.OnPartialResponseReceived += h,
                    h => client.OnPartialResponseReceived -= h)
                .Select(x => Enumerable.Repeat(RecognizedPhraseImpl.CreatePartial(x.EventArgs.PartialResult), 1));

            var responseObservable = Observable.FromEventPattern<SpeechResponseEventArgs>(
                    h => client.OnResponseReceived += h,
                    h => client.OnResponseReceived -= h)
                .Select(x =>
                {
                    var response = x.EventArgs.PhraseResponse;
                    switch (response.RecognitionStatus)
                    {
                        case RecognitionStatus.Intermediate:
                            return response.Results.Select(p => RecognizedPhraseImpl.CreateIntermediate(p));
                        case RecognitionStatus.RecognitionSuccess:
                            return response.Results.Select(p => RecognizedPhraseImpl.CreateSuccess(p));
                        case RecognitionStatus.EndOfDictation:
                            return s_thunk;
                        case RecognitionStatus.InitialSilenceTimeout:
                            throw new InitialSilenceTimeoutException();
                        case RecognitionStatus.BabbleTimeout:
                            throw new BabbleTimeoutException();
                        case RecognitionStatus.DictationEndSilenceTimeout:
                            throw new DictationEndTimeoutException();
                        case RecognitionStatus.Cancelled:
                        case RecognitionStatus.HotWordMaximumTime:
                        case RecognitionStatus.NoMatch:
                        case RecognitionStatus.None:
                        case RecognitionStatus.RecognitionError:
                        default:
                            throw new SpeechRecognitionException();
                    }
                })
                .TakeWhile(phrases => phrases != s_thunk);

            return responseObservable.Publish(observable =>
                Observable.Merge(errorObservable, partialObservable, observable)
                    .Window(() => observable));
        }

    }
}
