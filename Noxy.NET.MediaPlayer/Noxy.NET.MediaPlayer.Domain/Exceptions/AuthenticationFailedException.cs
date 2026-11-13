namespace Noxy.NET.MediaPlayer.Domain.Exceptions;

// Deliberately says nothing about why, so callers can't tell a wrong password from an unknown email.
public class AuthenticationFailedException() : Exception("Authentication failed.");
