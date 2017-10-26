# Cryptany Trusted Payment Network project

Cryptany Trusted Payment Network: make any cryptocurrency spendable.
Another approach to resolve major problems with integration of blockchain payments into realworld payment systems.
We have a whitepaper describing top level solution here: https://cryptany.io

## General
The project top-level architecture described in documents located in /docs directory.

For processing system we use .NET Framework 4 code from cellular mobile payments solution to get prototype (proof of concept) running as early as possible. Please dive deeper for more detailed description.

Cryptogateway part is written in PHP7 with modern Laravel5/Lumen framework.
Blockchain adapters (Ethereum as a first) is written in JavaScript utilizing native web3 library and is running under Node.js server.
It is located under crypto dir.

Smart contract is written in Solidity and is stored under /contracts dir.

## Requirements

## Building

## Running
