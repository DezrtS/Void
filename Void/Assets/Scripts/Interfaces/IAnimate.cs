public interface IAnimate
{
    public enum AnimationEventType
    {
        Trigger,
        Bool,
        Float,
    }

    public delegate void AnimationEventHandler(AnimationEventType animationEventType, string name, string value);
    public event AnimationEventHandler OnAnimationEvent;
}