using System.Net.WebSockets;
using System.Text;

var websocket = new ClientWebSocket();
string name;
Console.WriteLine($"Input Name: ");
name = Console.ReadLine();;
Console.WriteLine("Connecting To server...");
Thread.Sleep(5000);
await websocket.ConnectAsync(new Uri($"ws://localhost:5022/ws?name={name}"),CancellationToken.None);
Console.WriteLine("Connected...");
var sendTask = Task.Run(async () => 
{
    while (true)
    {
        var message = Console.ReadLine().Trim();
        if (message == "exit")
        {
            break;
        }
        var buffer = System.Text.Encoding.UTF8.GetBytes(message);
        await websocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text,true,CancellationToken.None);
        
    }
});
// var buffer = new byte[1024 *5];
// while (true)
// {
//     var result = await websocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
//     if (result.MessageType == WebSocketMessageType.Close)
//     {
//         break; 
//     }
//     var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
//     Console.WriteLine("Received:" + message);
// }
var receiveTask = Task.Run(async() =>
{
    var buffer = new byte[1024 * 5];
    while (true)
    {
        var result = await websocket.ReceiveAsync(new ArraySegment<byte>(buffer),CancellationToken.None);
        if (result.MessageType == WebSocketMessageType.Close)
        {
            break;
        }
        var message = Encoding.UTF8.GetString(buffer,0,result.Count).Trim();
        Console.WriteLine("Received:" +  message);
    }
});

await Task.WhenAny(receiveTask,sendTask);
if (websocket.State != WebSocketState.Closed)
{
    await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure,"Closing",CancellationToken.None);
}
await Task.WhenAll(receiveTask,sendTask);