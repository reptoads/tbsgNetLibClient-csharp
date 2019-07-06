
namespace NetLib{

    public enum NetCommands
    {
	/**
	 * \brief Sent by client. First command the client should sent to the Lobby or Gameserver to identify itself.
	 * \param std::string The session token of the client that it got provided from the Login WebApp
	 */
	Identify,
	/**
	 * \brief Sent by server after NetCommands::Identify. Sent when the identification was valid.
	 */
	IdentifySuccessful,
	/**
	 * \brief Sent by server after NetCommands::Identify. Sent when the identification was invalid.
	 */
	IdentifyFailure,
	/**
	 * \brief Sent by server if the client is trying to execute a custom command while not being identified.
	 */
	NotIdentified,

	/**
	 * \brief Sent by server if the client has connected but the server has encryption disabled.
	 */
	ConnectedWithoutEncryption,

	HandshakeServerKey,
	HandshakeDataKey,
	HandshakeSuccess,
	HandshakeFailed,

	CryptoPacket,
    /**
	 * \brief Custom command that will be passed to the custom implementation.
	 */
	CustomCommand    
}

}