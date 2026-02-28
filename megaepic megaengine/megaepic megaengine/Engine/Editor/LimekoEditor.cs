namespace Limeko.Editor
{
    public static class EditorPlayer
    {
        public static bool isPlaying = false;
        public static bool isPaused = false;

        private static int velIterateCount = 8;


        public static void TogglePlaymode()
        {
            isPlaying = !isPlaying;
            I_PlayingChanged();
        }

        public static void PausePlaymode()
        {
            isPaused = !isPaused;
            I_OnPauseChanged();
        }


        private static void I_PlayingChanged()
        {
            if(isPlaying)
            {
                
                Physics.Initialize();
            }
            else Physics.Dispose();
        }

        private static void I_OnPauseChanged()
        {
            Console.WriteLine("EditorPlayer Warn: PlayMode pause is not implemented.");
        }
    }
}