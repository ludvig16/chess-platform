namespace ChessPlatform.Users;

public class UserService
{
    private readonly UserRepository _userRepository;
    
    public UserService(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public Task<List<User>> GetAllAsync()
    {
        return _userRepository.GetAllAsync();
    }

    public ValueTask<User?> GetUserByIdAsync(int id)
    {
        return _userRepository.GetUserByIdAsync(id);
    }
}