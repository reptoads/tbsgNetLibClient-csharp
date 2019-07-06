

# Example use of the WebApi

## Reference of `enum`

```csharp
namespace tbsgNetLib.WebAPI
{
    public enum WebMethod
    {
        GET = 0,
        POST = 1
    };

    public enum Service
    {
        None,
        Login,
        Data,
        Register,
        ValidateToken,
        UserDeck
    };
 [...]
 }
```
Source: [`WebAPI/APIRequest.cs`](https://github.com/reptoads/tbsgNetLibClient-csharp/blob/master/WebAPI/APIRequest.cs)

## ApiRequest

```csharp
   class Program
    {
        static void Main(string[] args)
        {
            Packet packet = new Packet();
            ApiRequest.Create("https://mydoamin.com", 412);
            ApiRequest.AddService(Service.Login, "/v1/login.php");
            ApiRequest.AddService(Service.ValidateToken, "/v1/checkSession.php");
            ApiRequest.AddService(Service.Data, "/v1/getData.php");
            ApiRequest.AddService(Service.UserDeck, "/v1/getDeck.php");
            
            if (ApiRequest.CheckConnection())
            {
                [...]
            }
        }
    }
```



## Login System



```csharp
            if (ApiRequest.CheckConnection())
            {
                Login.SessionFile = "access.token"; // for storing the access token
                Session session = Login.Request("username", "password");
                Login.SaveSessionToDisk(session, true); // stores as binary on the HDD
            }
```



Load `access.token` file from HDD in order to use it instead of logging

```csharp
            if (ApiRequest.CheckConnection())
            {
                Login.SessionFile = "access.token";
                var session = Login.LoadLocalSessionFromDisk();
                Login.SaveSessionToDisk(session, true);
            }
```



