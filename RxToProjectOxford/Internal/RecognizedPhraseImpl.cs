//
// Copyright Microsoft Corporation.  All rights reserved.
// ericroz - Sept 2015
//

using MicrosoftProjectOxford;

namespace RxToProjectOxford
{
    abstract class RecognizedPhraseImpl : IRecognizedPhrase
    {
        private RecognizedPhraseImpl(string lexicalForm)
        {
            LexicalForm = lexicalForm;
        }

        public string LexicalForm { get; }

        public abstract RecognizedPhraseKind Kind { get; }

        public abstract Confidence Confidence { get; }

        public abstract string DisplayText { get; }

        public abstract string InverseTextNormalizationResult { get; }

        public abstract string MaskedInverseTextNormalizationResult { get; }

        public static IRecognizedPhrase CreateIntermediate(MicrosoftProjectOxford.RecognizedPhrase phrase)
        {
            return new OxfordPhraseImpl(phrase, RecognizedPhraseKind.Intermediate);
        }

        public static IRecognizedPhrase CreateSuccess(MicrosoftProjectOxford.RecognizedPhrase phrase)
        {
            return new OxfordPhraseImpl(phrase, RecognizedPhraseKind.Success);
        }

        public static IRecognizedPhrase CreatePartial(string partialResult)
        {
            return new PartialPhraseImpl(partialResult);
        }

        class OxfordPhraseImpl : RecognizedPhraseImpl
        {
            public OxfordPhraseImpl(MicrosoftProjectOxford.RecognizedPhrase phrase, RecognizedPhraseKind kind)
                : base(phrase.LexicalForm)
            {
                Kind = kind;
                Confidence = phrase.Confidence;
                DisplayText = phrase.DisplayText;
                InverseTextNormalizationResult = phrase.InverseTextNormalizationResult;
                MaskedInverseTextNormalizationResult = phrase.MaskedInverseTextNormalizationResult;
            }

            public override RecognizedPhraseKind Kind { get; }

            public override Confidence Confidence { get; }

            public override string DisplayText { get; }

            public override string InverseTextNormalizationResult { get; }

            public override string MaskedInverseTextNormalizationResult { get; }
        }

        class PartialPhraseImpl : RecognizedPhraseImpl
        {
            public PartialPhraseImpl(string partialResult)
                : base(partialResult)
            {
            }

            public override RecognizedPhraseKind Kind { get { return RecognizedPhraseKind.Partial; } }

            public override Confidence Confidence { get { return Confidence.None; } }

            public override string DisplayText { get { return null; } }

            public override string InverseTextNormalizationResult { get { return null; } }

            public override string MaskedInverseTextNormalizationResult { get { return null; } }
        }
    }
}
