use test;

show tables;

create database peach;

use peach;

create table Mutator(
	id INT auto_increment,    
    name CHAR(100),
    primary key(id));

insert into Mutator (name) values ('StringBenefitMutator');
insert into Mutator (name) values ('UnicodeBadUtf8BenefitMutator');
insert into Mutator (name) values ('UnicodeBomBenefitMutator');
insert into Mutator (name) values ('UnicodeStringsBenefitMutator');
insert into Mutator (name) values ('UnicodeUtf8ThreeCharBenefitMutator');

create table Mutation(
    mutatorID INT,
    position INT,
    times INT,
    primary key(mutatorID, position));


select * from Mutation order by times DESC limit 0,10000;

select count(*) from Mutation limit 100000;

#delete from Mutation where mutatorID = 1;


create database peach;

drop database peach;
