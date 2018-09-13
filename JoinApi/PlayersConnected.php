<?php
class PlayersConnected{
 
    // database connection and table steam
    private $conn;
    private $table_name = "playerjoin";
 
    // object properties
    public $steam;
    public $timestamp;
 
    // constructor with $db as database connection
    public function __construct($db){
        $this->conn = $db;
    }
	// read Players
	function read(){
	 
		// select all query
		$query = "SELECT
					steam, timestamp
				FROM
					" . $this->table_name . "
				ORDER BY
					timestamp";
	 
		// prepare query statement
		$stmt = $this->conn->prepare($query);
	 
		// execute query
		$stmt->execute();
	 
		return $stmt;
	}
	// delete Player
	function delete(){
	 
		// delete query
		$query = "DELETE FROM " . $this->table_name . " WHERE steam = ?";
	 
		// prepare query
		$stmt = $this->conn->prepare($query);
	 
		// sanitize
		$this->steam=htmlspecialchars(strip_tags($this->steam));
	 
		// bind steam of record to delete
		$stmt->bindParam(1, $this->steam);
	 
		// execute query
		if($stmt->execute()){
			return true;
		}
	 
		return false;
		 
	}
	// create Player
	function create(){
	 
		// query to insert record
		$query = 
				"INSERT INTO " . $this->table_name . " (steam, timestamp)
				VALUES (:steam, :timestamp)
				ON DUPLICATE KEY 
				UPDATE timestamp = :timestamp";
	 
		// prepare query
		$stmt = $this->conn->prepare($query);
	 
		// sanitize
		$this->steam=htmlspecialchars(strip_tags($this->steam));
		$this->timestamp=htmlspecialchars(strip_tags($this->timestamp));
	 
		// bind values
		$stmt->bindParam(":steam", $this->steam);
		$stmt->bindParam(":timestamp", $this->timestamp);
	 
		// execute query
		if($stmt->execute()){
			return true;
		}
	 
		return false;
	}
	// update the product
	function update(){
	 
		// update query
		$query = "UPDATE
					" . $this->table_name . "
				SET
					timestamp = :timestamp
				WHERE
					steam = :steam";
	 
		// prepare query
		$stmt = $this->conn->prepare($query);
	 
		// sanitize
		$this->steam=htmlspecialchars(strip_tags($this->steam));
		$this->timestamp=htmlspecialchars(strip_tags($this->timestamp));
	 
		// bind new values
		$stmt->bindParam(":steam", $this->steam);
		$stmt->bindParam(":timestamp", $this->timestamp);
	 
		// execute query
		if($stmt->execute()){
			return true;
		}
	 
		return false;
	}
}