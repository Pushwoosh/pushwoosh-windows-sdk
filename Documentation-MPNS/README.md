# Class NotificationService #

Namespace `namespace PushSDK`

NotificationService class offers access to the singletone-instance of the push service responsible for registering the device with the Microsoft Push Notification Servers, receiving and processing push notifications.

## Class summary
[LastPushContent](#lastpushcontent) *property*  
[UserData](#userdata) *property*  
[PushToken](#pushtoken) *property*  
[DeviceUniqueID](#deviceuniqueid) *property*  

[public event EventHandler<ToastPush> OnPushAccepted;](#onpushaccepted) *event*  
[public event EventHandler<string> OnPushTokenReceived;](#onpushtokenreceived) *event*  
[public event EventHandler<string> OnPushTokenFailed;](#onpushtokenfailed) *event*  

[public static NotificationService GetCurrent();](#getcurrent)  
[public async void SubscribeToPushService()](#subscribetopushservice)  
[public void UnsubscribeFromPushes(EventHandler<JObject> success, EventHandler<string> failure)](#unsubscribefrompushes)  
[public void SendTag(IList<KeyValuePair<string, object>> tagList, EventHandler<JObject> OnTagSendSuccess, EventHandler<string> OnError)](#sendtag)  
[public void GetTags(EventHandler<JObject> OnTagsSuccess, EventHandler<string> OnError)](#gettags)  
[public void StartGeoLocation()](#startgeolocation)  
[public void StopGeoLocation()](#stopgeolocation)  
[public void TrackInAppPurchase(string productId, double price, string currency)](#trackinapppurchase)  


## Properties

### LastPushContent

Get content of last push notification.

```csharp
public string LastPushContent
```

---
### UserData

Get user data of last push notification.

```csharp
public string UserData
```

---
### PushToken

Returns push token. Can be null if no push tokens has been received yet.


```csharp
public string PushToken
```

---
### DeviceUniqueID

Gets Unique Device Identifier that is used in all API calls with Pushwoosh.

```csharp
public string DeviceUniqueID
```

---
## Events

### OnPushAccepted

Called when push notification has been tapped by user.

```csharp
public event EventHandler<ToastPush> OnPushAccepted;
```

---
### OnPushTokenReceived

Called when push notification token has been received.

```csharp
public event EventHandler<string> OnPushTokenReceived;
```

---
### OnPushTokenFailed

Called when push notification registration has failed.

```csharp
public event EventHandler<string> OnPushTokenFailed;
```

---
## Methods

### GetCurrent

Singletone access to the NotificationService. May return null if appID is unknown and have never been set before.

```csharp
public static NotificationService GetCurrent();
public static NotificationService GetCurrent(string appID, string serviceName, IEnumerable<string> tileTrustedServers);
```

---
### SubscribeToPushService

Creates push channel and registers push notification token with Pushwoosh server. Returns callback events via EventHandlers.

```csharp
public void SubscribeToPushService()
public void SubscribeToPushService(string serviceName)
```

---
### UnsubscribeFromPushes

Unsubscribes from push notifications and unregisters from Pushwoosh server.

```csharp
public void UnsubscribeFromPushes(EventHandler<JObject> success, EventHandler<string> failure)
```

---
### SendTag

Sends tags to Pushwoosh.

```csharp
public void SendTag(List<KeyValuePair<string, object>> tagList, EventHandler<JObject> OnTagSendSuccess, EventHandler<string> OnError)
```

---
### GetTags

Gets tags from Pushwoosh for the device.

```csharp
public void GetTags(EventHandler<JObject> OnTagsSuccess, EventHandler<string> OnError)
```
* **OnTagsSuccess** - JSON object with tag values.

---
### StartGeoLocation

Starts Geo Push notifications

```csharp
public void StartGeoLocation()
```

---
### StopGeoLocation

Stops Geo Push notifications

```csharp
public void StopGeoLocation()
```

---
### TrackInAppPurchase

Tracks In-App purchase.

```csharp
public void TrackInAppPurchase(string productId, double price, string currency)
```

* **productId** - Id of the product purchased
* **price** - Price of the product
* **currency** - Currency of the price
