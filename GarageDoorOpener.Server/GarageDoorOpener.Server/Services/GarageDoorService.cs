using GarageDoorOpener.Server.Interfaces;
using GarageDoorOpener.Shared.Protos;
using Grpc.Core;

namespace GarageDoorOpener.Server.Services
{
    internal class GarageDoorService : GarageDoor.GarageDoorBase
    {
        private readonly ILogger<GarageDoorService> _logger;
        private readonly IGarageDoorController _garageDoorController;
        private readonly IAuthenticatorService _authenticatorService;

        public GarageDoorService(ILogger<GarageDoorService> logger, IGarageDoorController garageDoorController, IAuthenticatorService authenticatorService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _garageDoorController = garageDoorController ?? throw new ArgumentNullException(nameof(garageDoorController));
            _authenticatorService = authenticatorService ?? throw new ArgumentNullException(nameof(authenticatorService));
        }
        public override async Task<SignalDoorReply> SignalDoor(SignalDoorRequest request, ServerCallContext context)
        {
            if(string.IsNullOrWhiteSpace(request.AuthenticationCode) || !_authenticatorService.Authenticate(request.AuthenticationCode))
            {
                _logger.LogInformation("Failed to authenticate the request");
                context.Status = new Status(StatusCode.Unauthenticated, "Could not authenticate the request");
                return new SignalDoorReply();
            }
            _logger.LogTrace("Start Open Door Request");
            await Process(request.SignalDoorCase);
            _logger.LogTrace("End Open Door Request");
            return new SignalDoorReply();
        }

        private Task Process(SignalDoorRequest.SignalDoorOneofCase openDoor) =>
            openDoor switch
            {
                SignalDoorRequest.SignalDoorOneofCase.OpenLeftDoor => ProcessLeftDoor(),
                SignalDoorRequest.SignalDoorOneofCase.OpenRightDoor => ProcessRightDoor(),
                SignalDoorRequest.SignalDoorOneofCase.None => throw new ArgumentException("Invalid request"),   //Adding this just to be thorough
                _ => throw new ArgumentException("Invalid request"),
            };

        private Task ProcessLeftDoor()
        {
            _logger.LogInformation("Signaling left door");
            return _garageDoorController.SignalLeftDoor();
        }

        private Task ProcessRightDoor()
        {
            _logger.LogInformation("Signaling right door");
            return _garageDoorController.SignalRightDoor();
        }
    }
}
