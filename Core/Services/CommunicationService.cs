using ProVoiceLedger.Core.Models;
using ProVoiceLedger.Core.Services;
namespace ProVoiceLedger.Core.Services
{
    public class CommunicationService
    {
        private readonly AuthService _authService;
        private readonly FileStorageService _fileStorageService;

        public CommunicationService(AuthService authService, FileStorageService fileStorageService)
        {
            _authService = authService;
            _fileStorageService = fileStorageService;
        }

        public LoginResponse HandleLogin(LoginRequest request)
        {
            // Use the AuthService to validate credentials and return the response directly
            return _authService.ValidateCredentials(request);
        }

        public bool HandleFileTransfer(FileTransferRequest request)
        {
            // Optional: validate session token
            if (!_authService.ValidateSessionToken(request.SessionToken))
                return false;

            return _fileStorageService.SaveFile(request.Filename, request.FileBytes);
        }
    }
}
