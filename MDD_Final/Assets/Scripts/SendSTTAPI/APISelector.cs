using UnityEngine;

public class APISelector : MonoBehaviour
{
    [SerializeField] private APIType apiType;
    
    /**
     * API 타입 Enum.
     */
    public enum APIType
    {
        Clova,
        Azure
    };

    /**
     * API 타입 Getter.
     */
    public APIType GetAPIType()
    {
        return this.apiType;
    }
}
