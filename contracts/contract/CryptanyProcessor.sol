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
    }
    
    //all payments agregated by this contract
    uint public balance;
    
    //rating(reputation) == sum of all success eth deals
    mapping (address => uint) rating;
    
    //mapping betwen transaction hash and payment structure
    mapping (bytes32 => PaymentStructure) pendingPayments;
    
    /**
     * @dev Throws if called by any account other than the owner.
     */
    modifier onlyOwner() {
        _;
    }
    
    
   /**
    * @dev event occurs in each transaction
    */
    event sendMoneyEvent(
        address indexed toPerson, 
        address indexed fromPerson,
        bytes32 transactionHash,
        uint payment,
        string comment,
        address[] judges
        );
        
   /**
    * @dev event occurs in each positive validation
    */    
    event transactionApproved(
        bytes32 indexed transactionHash,
        address validator,
        string comment
        );
        
   /**
    * @dev event occurs in each rejection
    */    
    event transactionRejected(
        bytes32 indexed transactionHash,
        address validator,
        string comment
        );        
        
        
   /**
    * @dev event occurs when new validatior is added/removed
    * 
    */    
    event changeListOfValidators(
        string operationType,
        address validator
        );
        

   /**
    * @dev Main function to create transaction
    */
    function sendMoneyFor(address toPerson, string comment, address[] judges) payable returns (bytes32) {
        
        //Seller balance should bigger, than any payment
        require (toPerson.balance >= msg.value);
        
        //var pendingPay = PaymentStructure(msg.sender, toPerson, msg.value, comment);
        var pendingPay = PaymentStructure(msg.sender, toPerson, msg.value, comment, judges);
        
        //hash from all concatinated data
        var transactionHash = sha3(msg.sender, toPerson, msg.value, block.timestamp);

        pendingPayments[transactionHash]=pendingPay;
        balance += msg.value;
        
        //sendMoneyEvent(toPerson, msg.sender, msg.value, comment, transactionHash);
        sendMoneyEvent(toPerson, msg.sender, transactionHash, msg.value, comment, judges);
        
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
        
        return true;
        
    }
    
   /**
    * @dev function to approve transaction. Should work with many validators.
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
    
    function tst (bytes32 transactionHash) {
        PaymentStructure memory pp = pendingPayments[transactionHash]; 
        tste(pp.validationsList.length);
    }
    
    event tste(
        uint size);

}