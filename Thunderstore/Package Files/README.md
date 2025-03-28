# EnemyAttackSpeedFixes

This fixes a few enemy attacks not scaling with attack speed:
- Stone golem's clap attack
- Mithrix's hammer slam

Weirdly enough in vanilla the sound for these attacks still scale with attack speed even though the attack itself doesn't, so these likely weren't intentional.

## Before

![before](https://github.com/user-attachments/assets/71a3dd6e-3ab5-402d-9e2c-53492cc6b4ea)

## After

![after](https://github.com/user-attachments/assets/00d86ea2-31a8-409f-99ba-d61e50ec973a)


### Known Issues

- Mithrix's hammer slam's ground explosion sometimes doesn't happen at VERY high attack speeds
- - Probably isn't possible to have happen in any normal vanilla or modded situations, AFAIK you'd have to voluntarily give Mithrix a bajillion attack speed to see it happen
- Mithrix's hammer side swipe's sound doesn't scale with attack speed
- - Currently not sure how I would fix it