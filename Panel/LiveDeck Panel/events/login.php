<?php
    // POST Parameter
    $username = $_POST["username"];
    $password = $_POST["password"];

    // Connection credentials
    $address = "127.0.0.1";
    $port = "1111";

    // Create socket
    $sock = socket_create(AF_INET,SOCK_STREAM,SOL_TCP) or die("<h1>No connection to auth service</h1>");
    // Connect to endpoint
    socket_connect($sock, $address, $port) or die("<h1>No connection to auth service</h1>");

    // Token request
    $request = new Request("login", $username, $password);

    // -> Write to auth server
    $msg = json_encode($request);
    socket_write($sock, $msg."\r\n");

    // Wait for response
    $response = socket_read($sock, 2048, PHP_BINARY_READ);
    socket_close($sock);

    // Decode response content
    $decodedResponse = json_decode($response);
    CheckResponse($decodedResponse, $username);

    class Request
    {
        public string $Type;
        public string $Username;
        public string $Password;

        public function __construct(string $Type, string $Username, string $Password)
        {
            $this->Type = $Type;
            $this->Username = $Username;
            $this->Password = $Password;
        }

        function set_Password($Password)
        {
            $this->Password = $Password;
        }
        function get_Password(): string
        {
            return $this->Password;
        }

        function set_Type($Type)
        {
            $this->Type = $Type;
        }
        function get_Type(): string
        {
            return $this->Type;
        }

        function set_Username($Username)
        {
            $this->Username = $Username;
        }
        function get_Username(): string
        {
            return $this->Username;
        }
    }

    function CheckResponse($response, $username): void
    {
        if ($response -> {'Result'} == "Success") {
            $utoken = $response -> {'UToken'};
            setcookie("utoken", $utoken, time() + 3600, "/");
            header('Location: ../deck/'.$response -> {'Token'}.'/'.$username.'.php');
        } else if ($response -> {'Type'} == "Error"){
            header('Location: ../?error='.$response -> {'Reason'});
        } else if ($response -> {'Result'} == "Admin") {
            $admintoken = $response -> {'AdminToken'};
            setcookie("AdminToken", $admintoken, time() + 3600, "/");

            // Enter your website domain
            setcookie("AdminToken", $admintoken, time() + 3600, "/", ".[your domain]");
            
            // Enter your website sub-domain for the admin panel example: https://admin.example.com/
            header('Location: https://[your sub domain]');
        }
    }

    die();