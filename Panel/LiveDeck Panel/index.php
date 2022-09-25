<?php

// Check if AdminToken is in cookies
if (isset($_COOKIE["AdminToken"])) {

    // Check validation of AdminToken
    $adminToken = $_COOKIE["AdminToken"];

    // Enter your website domain
    $response = file_get_contents('https://[your domain]/api/admin/validate?AdminToken=' . $adminToken);
    $validation = json_decode($response);

    if ($validation->{"Valid"}) {
        // Enter your website sub-domain for the admin panel example: https://admin.example.com/
        header('Location: https://[your sub domain]/');
        die;
    }
}

?>

<!DOCTYPE html>
<html lang="en">
    <head>
        <!-- META -->
        <meta charset="utf-8">
        <meta http-equiv="X-UA-Compatible" content="IE=edge">
        <meta name="viewport" content="width=device-width, initial-scale=1">
        <!--<meta name="description" content=""/>-->
        <!-- META -->

        <title>Live-Deck</title>

        <!-- LINK -->
        <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css">
        <link rel="stylesheet" href="https://netdna.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css">
        <link rel="stylesheet" href="assets/stylesheets/style.css">
        <link rel="shortcut icon" href="assets/img/logo_ld_livedeck_1.ico">
        <!-- LINK -->

        <!-- PWA -->
        <meta name="apple-mobile-web-app-capable" content="yes">
        <link rel="manifest" href="manifest.json">

        <link rel="apple-touch-icon" href="assets/img/logo_ld_livedeck_2.png">
        <link rel="apple-touch-icon" sizes="152x152" href="assets/img/logo_ld_livedeck_2.png">
        <link rel="apple-touch-icon" sizes="180x180" href="assets/img/logo_ld_livedeck_2.png">
        <link rel="apple-touch-icon" sizes="167x167" href="assets/img/logo_ld_livedeck_2.png">

        <meta name="apple-mobile-web-app-status-bar-style" content="default">
        <!-- PWA -->
    </head>
    <body>
        <div class="container ">
            <div class="row">
                <div class="login-container col-lg-4 col-md-6 col-sm-8 col-xs-12">
                    <div class="login-title text-center">
                        <img width="86" src="assets/img/logo_ld_livedeck_1.ico">
                        <h2><span>Live<strong>Deck</strong></span></h2>
                    </div>
                    <?php
                    if (isset($_GET["error"])) {
                        $ErrorType = $_GET["error"];

                        if ($ErrorType == "USER_NOT_EXIST") {
                            echo "<div class='error'><h4>User not exist</h4></div>";
                        } else if ($ErrorType == "PASSWORD_INCORRECT") {
                            echo "<div class='error'><h4>Password is wrong</h4></div>";
                        }
                    }

                    ?>
                    <div class="login-content">
                        <div class="login-header">
                            <h3><strong>Welcome</strong></h3>
                        </div>
                        <div class="login-body">
                            <form role="form" action="events/login.php" method="post" class="login-form">
                                <div class="form-group ">
                                    <div class="pos-r">
                                        <input required id="username" type="text" name="username" placeholder="Username..." class="form-username form-control">
                                        <i class="fa fa-user"></i>
                                        <span></span>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <div class="pos-r">
                                        <input required id="password" type="password" name="password" placeholder="Password..." class="form-password form-control" >
                                        <i class="fa fa-lock"></i>
                                        <span></span>
                                    </div>
                                </div>
                                <div class="form-group text-right">
                                    <a href="#" class="bold">Get your login</a>
                                </div>

                                <div class="form-group">
                                    <button type="submit" class="btn btn-warning form-control"><strong>Log In</strong></button>
                                </div>
                            </form>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <script>
            var a=document.getElementsByTagName("a");
            for(var i=0;i<a.length;i++)
            {
                a[i].onclick=function()
                {
                    window.location=this.getAttribute("href");
                    return false
                }
            }
        </script>

        <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.2.1/jquery.min.js"></script>
        <script src="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/js/bootstrap.min.js"></script>
    </body>
</html>
