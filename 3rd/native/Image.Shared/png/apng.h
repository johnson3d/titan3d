#ifndef APNG_H
#define APNG_H

#include "png.h"
#include "pngstruct.h"
#include "pngpriv.h"
#include "pngdebug.h"


#define APNG_DISPOSE_OP_NONE		0	/*no disposal is done on this frame before rendering the next; the contents of the output buffer are left as is. */
#define APNG_DISPOSE_OP_BACKGROUND	1	/*the frame's region of the output buffer is to be cleared to fully transparent black before rendering the next frame. */
#define APNG_DISPOSE_OP_PREVIOUS	2	/*the frame's region of the output buffer is to be reverted to the previous contents before rendering the next frame. */

#define APNG_BLEND_OP_SOURCE		0	/*all color components of the frame, including alpha, overwrite the current contents of the frame's output buffer region */
#define APNG_BLEND_OP_OVER			1	/*the frame should be composited onto the output buffer based on its alpha, using a simple OVER operation as described in the "Alpha Channel Processing" section of the PNG specification [PNG-1.2]. Note that the second variation of the sample code is applicable. */

/*`acTL`*/
typedef struct apng_animation_control_chunk
{
	png_uint_32			num_frames;		/*Number of frames*/
	png_uint_32			num_plays;		/*Number of times to loop this APNG.  0 indicates infinite looping.*/
} apng_animation_control_chunk;

/*`fcTL`*/
typedef struct apng_frame_control_chunk
{
	png_uint_32			sequence_number;/*Sequence number of the animation chunk, starting from 0*/
	png_uint_32			width;			/*Width of the following frame*/
	png_uint_32			height;			/*Height of the following frame*/
	png_uint_32			x_offset;		/*X position at which to render the following frame*/
	png_uint_32			y_offset;		/*Y position at which to render the following frame*/
	png_uint_32			delay_num;		/*Frame delay fraction numerator*/
	png_uint_32			delay_den;		/*Frame delay fraction denominator*/
	png_byte			dispose_op;		/*Type of frame area disposal to be done after rendering this frame*/
	png_byte			blend_op;		/*Type of frame area rendering for this frame*/
} apng_frame_control_chunk;

/*`fdAT`*/
typedef struct apng_frame_data_chunk
{
	png_uint_32			sequence_number;/*Sequence number of the animation chunk, starting from 0*/
	png_byte			frame_data[1];	/*Frame data for this frame*/
} apng_frame_data_chunk;

#endif /*APNG_H*/