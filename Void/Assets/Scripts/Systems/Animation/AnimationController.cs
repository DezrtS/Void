using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [Serializable]
    public class AnimatorInstance
    {
        [SerializeField] private string name;
        [SerializeField] private Animator animator;

        public AnimatorInstance(string name, Animator animator)
        {
            this.name = name;
            this.animator = animator;
        }

        public string Name => name;
        public Animator Animator => animator;
    }

    [SerializeField] private List<AnimatorInstance> animatorInstances = new List<AnimatorInstance>();

    public AnimatorInstance AddAnimatorInstance(string name, Animator animator)
    {
        AnimatorInstance animatorInstance = new AnimatorInstance(name, animator);
        animatorInstances.Add(animatorInstance);
        return animatorInstance;
    }

    public void RemoveAnimatorInstance(AnimatorInstance animatorInstance)
    {
        animatorInstances.Remove(animatorInstance);
    }

    public void RemoveAnimatorInstance(string name)
    {
        for (int i = 0; i < animatorInstances.Count; i++)
        {
            AnimatorInstance instance = animatorInstances[i];
            if (instance.Name == name)
            {
                animatorInstances.RemoveAt(i);
                i--;
            }
        }
    }

    public void RemoveAnimatorInstance(Animator animator)
    {
        for (int i = 0; i < animatorInstances.Count; i++)
        {
            AnimatorInstance instance = animatorInstances[i];
            if (instance.Animator == animator)
            {
                animatorInstances.RemoveAt(i);
                return;
            }
        }
    }

    public void SetTrigger(string name)
    {
        foreach (AnimatorInstance animatorInstance in animatorInstances)
        {
            animatorInstance.Animator.SetTrigger(name);
        }
    }

    public void SetFloat(string name, float value)
    {
        foreach (AnimatorInstance animatorInstance in animatorInstances)
        {
            animatorInstance.Animator.SetFloat(name, value);
        }
    }
}
