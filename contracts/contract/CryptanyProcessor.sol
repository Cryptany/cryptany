pragma solidity ^0.4.11;

  /**
   * @dev Prototype for internal testing
   */
contract CryptanyProcessor {
    
  /**
   * @dev Structure for all incoming/outcoming payments
   */
    struct PaymentStructure {
    
        address fromAddress;

        address toAddress;

        uint paymentSize;
        
        string comment;
        
        address[] validationsList;
        
        uint timestamp;
    }
    
    //all payments agregated by this contract
    uint public balance;
    
    //Temporary time to rollback an unsuccessful transaction
    uint constant overdueTime = 3600000;
    
    //rating(reputation) == sum of all success eth deals
    mapping (address => uint) rating;
    
    //mapping betwen transaction hash and payment structure
    mapping (bytes32 => PaymentStructure) pendingPayments;
    
    
   /**
    * @dev event occurs in each transaction
    */
    event sendMoneyEvent(
        address indexed toPerson, 
        address indexed fromPerson,
        bytes32 transactionHash,
        uint payment,
        string comment,
        address[] trustbrokers
        );
        
   /**
    * @dev event occurs in each positive validation
    */    
    event transactionApproved(
        bytes32 indexed transactionHash,
        address trustbroker,
        string comment
        );
        
   /**
    * @dev event occurs in each rejection
    */    
    event transactionRejected(
        bytes32 indexed transactionHash,
        address trustbroker,
        string comment
        );        
        
        

   /**
    * @dev Main function to create transaction
    */
    function sendMoney(address toPerson, string comment, address[] trustbrokers) payable returns (bytes32) {
        
        //Seller balance should bigger, than any payment
        require (toPerson.balance >= msg.value);
        
        //hash from all concatinated data
        var transactionHash = keccak256(msg.sender, toPerson, msg.value, block.timestamp);
        
        //in case of we have no trustbrokers for ccurrent transaction it will be processed immediately
        if (trustbrokers.length == 0){
            toPerson.transfer(msg.value);  
            //lets increase rating for owners of current transaction
            changeRating(toPerson, msg.value);
            changeRating(msg.sender, msg.value);
        }
        else {
            //var pendingPay = PaymentStructure(msg.sender, toPerson, msg.value, comment);
            var pendingPay = PaymentStructure(msg.sender, toPerson, msg.value, comment, trustbrokers, block.timestamp);
    
            pendingPayments[transactionHash]=pendingPay;
            balance += msg.value;
            
            sendMoneyEvent(toPerson, msg.sender, transactionHash, msg.value, comment, trustbrokers);
            
        }
        
        return transactionHash;
    }
    
   /**
    * @dev Main function to process transaction
    */
    function receiveMoney(bytes32 transactionHash) returns (bool) {
        PaymentStructure memory pp = pendingPayments[transactionHash];
        
        //Only money receiver may call this function
        require (msg.sender == pp.toAddress);
        
        //only approved transaction may commited
        require (pp.validationsList.length == 0);
        
        //transaction from contract to address
        pp.toAddress.transfer(pp.paymentSize);
        
        //removing payment element
        delete pendingPayments[transactionHash];
        
        //lets increase rating for owners of current transaction
        changeRating(pp.toAddress, pp.paymentSize);
        changeRating(pp.fromAddress, pp.paymentSize);
        
        balance -= pp.paymentSize;
        
        return true;
        
    }
    
   /**
    * @dev function to approve transaction. Merchant will get his money
    */
    function provideValidation(bytes32 transactionHash, string comment) returns (bool){
        
        PaymentStructure memory pp = pendingPayments[transactionHash];

        for (uint i = 0; i<=pp.validationsList.length-1; i++){
            if (msg.sender == pp.validationsList[i]){
                delete pendingPayments[transactionHash].validationsList[i];
                pendingPayments[transactionHash].validationsList.length -= 1;
                transactionApproved(transactionHash, msg.sender, comment);
                return true;
            }
        }
        return false;    
    }
    
   /**
    * @dev function to rollback overdue transaction
    * may be rolled back after overdueTime
    */
    function rollbackvOverdueTransaction(bytes32 transactionHash) returns (bool) {
        PaymentStructure memory pp = pendingPayments[transactionHash];
        
        //Only money receiver may call this function
        require (msg.sender == pp.toAddress);
        
        uint deltaTime = block.timestamp - pp.timestamp;
        
        if (deltaTime >= overdueTime){
            
            //transaction from contract to address
            pp.fromAddress.transfer(pp.paymentSize);
            
            //removing payment element
            delete pendingPayments[transactionHash];
            
            balance -= pp.paymentSize;           
            return true;    
        }
        return false;
    }
    
   /**
    * @dev function to change rating of address
    */
    function changeRating(address _address, uint value) internal {
        rating[_address] += value;                
    }
    
   /**
    * @dev function to check rating of current address
    */
    function getRating(address _address) external returns (uint) {
        return rating[_address];
    }
    

}