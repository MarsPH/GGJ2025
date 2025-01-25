** Graphics are for reference only. This is for the blob and sticky effects **
Assign the tag of "Sticky" to objects or tilemaps to make him stick to them.
Assign the tag of "DetachOne" (As assigned to the explosion effect) to make him retach from walls or other objects.
There are certain situation where you want to have a moving object detach the stuck object, in that case assign a tag of "DetachAll". What that will do is make all attached points detach as soon as any of them are touched by the "DetachAll" object.
Note, when he sticks it's doing so by freezing the rigidbodies so he won't stick to moving platforms.
Replace "Blobby Sprite" with your graphic. It helps to make a copy of the current png and edit the desired image over so as to leave empty space at the edged, for a better squish effect.

*Notes on "Blob" script*
Spring frecquency = Lower is more squishy. Higher is more bouncy. Careful, too high and he deforms
Adjust all allReferencePoints and referencePointsCount in tandem to avoid errors.
Thanks for the support and enjoy!