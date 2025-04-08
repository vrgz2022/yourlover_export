using UnityEngine;

public class CharacterSystemSimple : MonoBehaviour
{
    public Transform characterParent;
    public Transform characterRoot;
    public Animator animator;
    public float rotationSpeed = 100f;
    public float moveSpeed = 0.5f;
    public float checkInterval = 0.5f;
    public string faceParameterName = "FaceState";
}

public class MouseFollower : MonoBehaviour
{
    public float FollowSpeed = 5f;
    public bool IsEnabled = true;
} 