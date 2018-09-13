<?php
// required headers
header("Access-Control-Allow-Origin: *");
header("Content-Type: application/json; charset=UTF-8");
header("Access-Control-Allow-Methods: POST");
header("Access-Control-Max-Age: 3600");
header("Access-Control-Allow-Headers: Content-Type, Access-Control-Allow-Headers, Authorization, X-Requested-With");
 
// get database connection
include_once './Database.php';
 
// instantiate PlayersConnected object
include_once './PlayersConnected.php';
 
$database = new Database();
$db = $database->getConnection();
 
$PlayersConnected = new PlayersConnected($db);
 
// get posted data
$data = json_decode(file_get_contents("php://input"));
 
// set PlayersConnected property values
$PlayersConnected->steam = $data->steam;
$PlayersConnected->timestamp = date('Y-m-d H:i:s');
 
// create the PlayersConnected
if($PlayersConnected->create()){
    echo '{';
        echo '"message": "Player was created."';
    echo '}';
}
 
// if unable to create the PlayersConnected, tell the user
else{
    echo '{';
        echo '"message": "Unable to create Player."';
    echo '}';
}
?>