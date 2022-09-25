<?php
    setcookie("AdminToken", "", time() - 3600, "/");

    // Enter your website domain
    setcookie("AdminToken", "", time() - 3600, "/", ".[your domain]");
    
    unset ($_COOKIE['AdminToken']);

    // Enter your website domain
    header('Location: https://[your domain]');