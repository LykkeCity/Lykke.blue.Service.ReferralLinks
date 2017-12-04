# Lykke.blue.Service.ReferralLinks

This repository holds the service containing all relevant endpoints for the Lykke.blue Referral Links functionality. 
It allows the users to create referral links which can be shared with friends.
Each link is created through the [Firebase API](https://firebase.google.com/) API and is preserved in the our own storage as well. When a user follows the link, either the Lykke.blue application is launched or the user is brough to the respective App/Play store to download the app.

Two types of links can be created:
* Invitation - invite a friend to join install the app and get into the community
    * Can be consumed by unlimited amount of people.
    * Has no expiration date
* Gift - send over an arbitrary amount of TREE coins via link.
    * Can be consumed only once
    * Expires after 30 days of not being used