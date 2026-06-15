using UnityEngine;

namespace Code.Core.Feedbacks
{
    public abstract class Feedback : MonoBehaviour
    {
        public abstract void CreateFeedback();
        public abstract void StopFeedback();
    }
}