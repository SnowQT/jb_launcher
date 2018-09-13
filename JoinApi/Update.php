<?php
// required headers
header("Access-Control-Allow-Origin: *");
header("Content-Type: application/json; charset=UTF-8");
header("Access-Control-Allow-Methods: POST");
header("Access-Control-Max-Age: 3600");
header("Access-Control-Allow-Headers: Content-Type, Access-Control-Allow-Headers, Authorization, X-Requested-With");
 
// include database and object files
include_once './Database.php';
include_once './PlayersConnected.php';
 
// get database connection
$database = new Database();
$db = $database->getConnection();
 
// prepare PlayersConnected object
$PlayersConnected = new PlayersConnected($db);
 
// get id of PlayersConnected to be edited
$data = json_decode(file_get_contents("php://input"));
 
// set ID property of PlayersConnected to be edited
$PlayersConnected->steam = $data->steam;
 
// set PlayersConnected property values
$PlayersConnected->timestamp = $data->timestamp;
 
// update the PlayersConnected
if($PlayersConnected->update()){
    echo '{';
        echo '"message": "Player was updated."';
    echo '}';
}
 
// if unable to update the PlayersConnected, tell the user
else{
    echo '{';
        echo '"message": "Unable to update Player."';
    echo '}';
}
?>