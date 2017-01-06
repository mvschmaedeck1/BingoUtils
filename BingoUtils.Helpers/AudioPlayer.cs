﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingoUtils.Helpers
{
    using System.ComponentModel;
    using System.Speech.Synthesis;
    using System.Threading;

    namespace BingoUtils.Helpers
    {
        public static class AudioPlayer
        {
            private static SpeechSynthesizer _Synthesizer = new SpeechSynthesizer() { Rate = 3, Volume = 100 };
            private static Prompt CurrentSpeak;

            public static void PlaySpeech(string speech)
            {
                CurrentSpeak = _Synthesizer.SpeakAsync(speech);
            }

            public static void StopSpeach()
            {
                if(CurrentSpeak == null)
                {
                    return;
                }

                _Synthesizer.SpeakAsyncCancel(CurrentSpeak);
                CurrentSpeak = null;
            }
        }
    }

}
