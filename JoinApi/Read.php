<?php
// required headers
header("Access-Control-Allow-Origin: *");
header("Content-Type: application/json; charset=UTF-8");
 
// include database and object files
include_once './Database.php';
include_once './PlayersConnected.php';
 
// instantiate database and PlayersConnected object
$database = new Database();
$db = $database->getConnection();
 
// initialize object
$PlayersConnected = new PlayersConnected($db);
 
// query products
$stmt = $PlayersConnected->read();
$num = $stmt->rowCount();
 
// check if more than 0 record found
if($num>0){
 
    // products array
    $player_arr=array();
    $player_arr["records"]=array();
 
    // retrieve our table contents
    // fetch() is faster than fetchAll()
    // http://stackoverflow.com/questions/2770630/pdofetchall-vs-pdofetch-in-a-loop
    while ($row = $stmt->fetch(PDO::FETCH_ASSOC)){
        // extract row
        // this will make $row['name'] to
        // just $name only
        extract($row);
 
        $player=array(
            "steam" => $steam,
            "timestamp" => $timestamp
        );

 
        array_push($player_arr["records"], $player);
		
    }
    echo json_encode($player_arr);
}
 
else{
    echo json_encode(
        array("message" => "No players found.")
    );
}
?>