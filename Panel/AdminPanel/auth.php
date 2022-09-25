<?php
    function Check() {
        // Check if AdminToken is in cookies
        if (!isset($_COOKIE["AdminToken"])) {
            header('Location: https://[your domain]');
        }

        // Check validation of AdminToken
        $adminToken = $_COOKIE["AdminToken"];
       
        // Enter valid api url
        $response = file_get_contents('http://[your domain]/api/admin/validate?AdminToken='.$adminToken);
        
        $validation = json_decode($response);

        if (!($validation -> {"Valid"})) {
            setcookie("AdminToken", "", time() - 3600, "/");

            // Enter your website domain
            setcookie("AdminToken", "", time() - 3600, "/", "[your domain]");
            
            header('Location: https://[your domain]');
        }
    }