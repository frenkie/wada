namespace Wada
{
    public class ReviveVideoInstruction : VideoInstruction
    {
        public float Delay;

        void Start()
        {
            Invoke( "FadeIn", Delay );
        }

        void OnFadeOut()
        {
            player.Stop();
            // don't call the base one
        }
    }
}