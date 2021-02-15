using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.IO;

namespace TimerBot
{
    class Program
    {
        public static string[] token;                // 토큰
        public static string[] prefix;               // 구분자

        DiscordSocketClient client;         // 봇 클라이언트
        CommandService commands;            // 명령어 수신 클라이언트

        static void Main(string[] args)
        {
            token = File.ReadAllLines(@"\token.txt");
            prefix = File.ReadAllLines(@"\prefix.txt");

            Console.WriteLine(token[0]);
            Console.WriteLine(prefix[0].ToCharArray()[0]);

            // 봇의 진입점 실행
            new Program().BotMain().GetAwaiter().GetResult();       
        }

        void update()
        {
        
        }

        // 봇의 진입점, 비동기 작업
        public async Task BotMain()
        {
            // 디스코드 봇 초기화
            client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose
            });

            // 명령어 수신 클라이언트 초기화
            commands = new CommandService(new CommandServiceConfig()
            {
                LogLevel = LogSeverity.Verbose
            });

            // 로그 수신 시 출력
            client.Log += OnClientLogReceived;
            commands.Log += OnClientLogReceived;

            // 봇의 토큰 사용 서버 로그인
            await client.LoginAsync(TokenType.Bot, token[0]);
            
            // 봇이 이벤트를 수신
            await client.StartAsync();

            // 봇이 메시지 수신 처리
            client.MessageReceived += OnClientMessage;

            // 봇에 명령어 모듈 등록
            await commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: null);

            // 봇이 종료되지 않도록 블로킹
            await Task.Delay(-1);

            // 봇 연결상태 체크
            ConnectionMonitor connectionMonitor = new ConnectionMonitor(client);
            connectionMonitor.Start();
        }

        private async Task OnClientMessage(SocketMessage msg)
        {
            var message = msg as SocketUserMessage;
            if (message == null)
                return;

            int pos = 0;

            // 메시지 앞에 !이 달려있지 않고, 자신이 아니면 호출 취소 처리
            if (!(message.HasCharPrefix(prefix[0].ToCharArray()[0], ref pos) || 
                message.HasMentionPrefix(client.CurrentUser, ref pos)) ||
                message.Author.IsBot)
                return;

            string[] text = message.Content.Trim().Split(' ');
            BasicModule.SetText(text);
                
            // 수신된 메시지 컨텍스트 생성
            ICommandContext context = new SocketCommandContext(client, message);

            //await Command(text, context);

            // 수신된 명령어 다시 보냄
            //await context.Channel.SendMessageAsync("명령어 수신 : " + message.Content);
            // 모듈이 명령어를 처리하게 설정
            var result = await commands.ExecuteAsync(context: context, argPos: pos, services: null);
        }

        private Task OnClientLogReceived(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
