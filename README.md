# mq-dotnet-administration-with-mqrestapi 
## Invoke IBM MQ REST API's using .NET application

IBM MQ Administration can be done programmatically using PCF.PCF is supported only with the Java and C client's of IBM MQ.

If we have to use .NET for IBM MQ Administration then there are multiple ways of doing it.One of them is by invoking
IBM MQ REST API's from a .NET application.To invoke MQ REST API's the MQ webserver must be running.
The REST API's return a JSON response which on received can be parsed and processed accordingly.

The applications 'MQDnetQueueAdminRestAPI' & MQDnetChannelAdminRestAPI' are a simple console application which demonstrates on how to GET/POST Queue
and Channel details of a Queue Manager.The application establishes a secure connection(HTTPS) to the Queue Manager if certificate details are provided or uses
HTTP if no certificate details are provided.

Following are the parameters that have to be passed to the 'MQDnetQueueAdminRestAPI' application:
```sh                                                                      
-qmgr : QueueManager
-queue : QueueName
-certDetails : certificate keydatabase along with password separated with '|'
-verb : REST API VERB(GET/POST)
-userCredentials: User credentials , username followed with password separated with ':'
```
Following are the parameters that have to be passed to the 'MQDnetQueueAdminRestAPI' application:
```sh                                                                      
-qmgr : QueueManager
-channel : Channel Name for GET
-command : runmqsc command for POST/GET
-certDetails : certificate keydatabase along with password separated with '|'
-verb : REST API VERB(GET/POST)
-userCredentials: User credentials , username followed with password separated with ':'
```

### How to run the application

#### Administration of Queue
```sh
dotnet MQDnetQueueAdminRestAPI.dll -qmgr QM1 -verb GET -queue Q1 -certDetails user.p12|password
```

If the application is invoked to get the basic details of Q1 which is on Queue Manager QM1 then the following command can be used

```sh
dotnet MQDnetQueueAdminRestAPI.dll -qmgr QM1 -verb GET -queue Q1 -userCredentials mqadmin:mqadmin
```

Output:

```sh                                                                      
Invoking :http://localhost:9080/ibmmq/rest/v1/admin/qmgr/QM1/queue/Q1
{"queue": [{
  "name": "Q1",
  "type": "local"
}]}
StatusCode: 200, ReasonPhrase: 'OK', Version: 1.1, Content: System.Net.Http.HttpConnectionResponseContent, Headers:
{
  Date: Mon, 08 Nov 2021 11:24:29 GMT
  Content-Type: application/json; charset=utf-8
  Content-Language: en-IN
  Content-Length: 50
}
```

To get more details of Q1 then use -queue Q1?attributes=*

```sh
dotnet MQDotnetRestAPI.dll -qmgr QM1 -verb GET -queue Q1?attributes=* -userCredentials mqadmin:mqadmin
```

Output:

```sh                                                                      
Invoking :http://localhost:9080/ibmmq/rest/v1/admin/qmgr/QM1/queue/Q1?attributes=*
{"queue": [{
  "cluster": {
    "namelist": "",
    "name": "",
    "workloadQueueUse": "asQmgr",
    "workloadPriority": 0,
    "workloadRank": 0,
    "transmissionQueueForChannelName": ""
  },
  "timestamps": {
    "created": "2021-03-04T05:30:10.000Z",
    "altered": "2021-10-13T07:34:47.000Z"
  },
  "storage": {
    "maximumDepth": 5000,
    "maximumMessageLength": 4194304,
    "messageDeliverySequence": "priority",
    "nonPersistentMessageClass": "normal"
  },
  "trigger": {
    "initiationQueueName": "",
    "depth": 1,
    "data": "",
    "processName": "",
    "messagePriority": 0,
    "type": "first",
    "enabled": false
  },
  "type": "local",
  "extended": {
    "allowSharedInput": true,
    "enableMediaImageOperations": "asQmgr",
    "backoutRequeueQueueName": "",
    "custom": "",
    "supportDistributionLists": false,
    "backoutThreshold": 0
  },
  "general": {
    "inhibitPut": false,
    "inhibitGet": false,
    "isTransmissionQueue": false,
    "description": ""
  },
  "applicationDefaults": {
    "putResponse": "synchronous",
    "messagePriority": 0,
    "messagePersistence": "nonPersistent",
    "clusterBind": "onOpen",
    "sharedInput": true,
    "readAhead": "no",
    "messagePropertyControl": "compatible"
  },
  "name": "Q1",
  "events": {
    "serviceInterval": {
      "duration": 999999999,
      "okEnabled": false,
      "highEnabled": false
    },
    "depth": {
      "lowPercentage": 20,
      "highPercentage": 80,
      "lowEnabled": false,
      "fullEnabled": true,
      "highEnabled": false
    }
  },
  "dataCollection": {
    "accounting": "asQmgr",
    "monitoring": "asQmgr",
    "statistics": "asQmgr"
  }
}]}
StatusCode: 200, ReasonPhrase: 'OK', Version: 1.1, Content: System.Net.Http.HttpConnectionResponseContent, Headers:
{
  Date: Mon, 08 Nov 2021 11:47:25 GMT
  Content-Type: application/json; charset=utf-8
  Content-Language: en-IN
  Content-Length: 1676
}
```

The application can be used for GET/POST on the Queue details of a Queue Manager.
#### Administration of Channel

To get the details of SYSTEM.DEF.SENDER channel which is on Queue Manager QM1 then the following command has to be used
```sh
dotnet MQDnetChannelAdminRestAPI.dll -qmgr QM1 -verb GET -channel SYSTEM.DEF.SENDER -certDetails user.p12|password
```

Output:

```
Invoking :http://localhost:9080/ibmmq/rest/v1/admin/qmgr/QM1/channel/SYSTEM.DEF.SENDER
{"channel": [{
  "sender": {
    "transmissionQueueName": "",
    "connection": []
  },
  "name": "SYSTEM.DEF.SENDER",
  "type": "sender"
}]}
StatusCode: 200, ReasonPhrase: 'OK', Version: 1.1, Content: System.Net.Http.HttpConnectionResponseContent, Headers:
{
  Date: Thu, 09 Dec 2021 13:02:24 GMT
  Content-Type: application/json; charset=utf-8
  Content-Language: en-IN
  Content-Length: 141
}
```

To create a channel DOTNET.SVRCONN on Queue Manager QM1 then the following command has to be used
```
dotnet MQDnetChannelAdminRestAPI.dll -qmgr QM1 -verb POST -command "DEFINE CHANNEL(DOTNET.SVRCONN) CHLTYPE(SVRCONN)" -certDetails user.p12|password
```

Output:
```
Invoking :https://localhost:9443/ibmmq/rest/v2/admin/action/qmgr/QM1/mqsc
{"type":"runCommand","parameters":{"command":"DEFINE CHANNEL(DOTNET.SVRCONN) CHLTYPE(SVRCONN)"}}
{
  "commandResponse": [{
    "completionCode": 0,
    "reasonCode": 0,
    "text": ["AMQ8014I: IBM MQ channel created."]
  }],
  "overallReasonCode": 0,
  "overallCompletionCode": 0
}
StatusCode: 200, ReasonPhrase: 'OK', Version: 1.1, Content: System.Net.Http.HttpConnectionResponseContent, Headers:
{
  Date: Thu, 09 Dec 2021 13:34:09 GMT
  Content-Type: application/json; charset=utf-8
  Content-Language: en-IN
  Content-Length: 184
}
```

###
Follow the below links for setting up the IBM MQ Webserver

Configuring REST API: https://www.ibm.com/docs/en/ibm-mq/9.2?topic=configuring-mq-console-rest-api
HTTP Basic Authentication : https://www.ibm.com/docs/en/ibm-mq/9.2?topic=security-using-http-basic-authentication-rest-api
Security: https://www.ibm.com/docs/en/ibm-mq/9.2?topic=securing-mq-console-rest-api-security

