<?php
// required headers
header("Access-Control-Allow-Origin: *");
header("Content-Type: application/json; charset=UTF-8");
header("Access-Control-Allow-Methods: POST");
header("Access-Control-Max-Age: 3600");
header("Access-Control-Allow-Headers: Content-Type, Access-Control-Allow-Headers, Authorization, X-Requested-With");
 
 
// include database and object file
include_once './Database.php';
include_once './PlayersConnected.php';
 
// get database connection
$database = new Database();
$db = $database->getConnection();
 
// prepare PlayersConnected object
$PlayersConnected = new PlayersConnected($db);
// get PlayersConnected steam
$data = json_decode(file_get_contents("php://input"));
 
// set PlayersConnected steam to be deleted
$PlayersConnected->steam = $data->steam;
 
// delete the PlayersConnected
if($PlayersConnected->delete()){
    echo '{';
        echo '"message": "Player was deleted."';
    echo '}';
}
 
// if unable to delete the PlayersConnected
else{
    echo '{';
        echo '"message": "Unable to delete object."';
    echo '}';
}
?>