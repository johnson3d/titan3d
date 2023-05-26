/***************************************************************************
* Copyright (c) Johan Mabille, Sylvain Corlay, Wolf Vollprecht and         *
* Martin Renou                                                             *
* Copyright (c) QuantStack                                                 *
*                                                                          *
* Distributed under the terms of the BSD 3-Clause License.                 *
*                                                                          *
* The full license is in the file LICENSE, distributed with this software. *
****************************************************************************/

#ifndef XSIMD_NEON_FLOATX16_HPP
#define XSIMD_NEON_FLOATX16_HPP

#include "xsimd_base.hpp"
#include "xsimd_neon_bool.hpp"

namespace xsimd
{
    /*******************
     * batch<float, 16> *
     *******************/
    template <>
    struct simd_batch_traits<batch<float, 16>>
    {
        using value_type = float;
        static constexpr std::size_t size = 16;
        using batch_bool_type = batch_bool<float, 16>;
        static constexpr std::size_t align = XSIMD_DEFAULT_ALIGNMENT;
        using storage_type = float32x4x4_t;
    };

    template <>
    class batch<float, 16> : public simd_batch<batch<float, 16>>
    {
    public:

        using self_type = batch<float, 16>;
        using base_type = simd_batch<self_type>;
        using storage_type = typename base_type::storage_type;

        batch();
        explicit batch(const float* src);

        batch(const float* src, aligned_mode);
        batch(const float* src, unaligned_mode);

        batch(const storage_type& rhs);
        
        batch& operator=(const storage_type& rhs);

        operator storage_type() const;
        float32x4_t operator [](std::size_t index) const;


        XSIMD_DECLARE_LOAD_STORE_ALL(float, 16)
        XSIMD_DECLARE_LOAD_STORE_LONG(float, 16)

        using base_type::load_aligned;
        using base_type::load_unaligned;
        using base_type::store_aligned;
        using base_type::store_unaligned;
    };
    
    XSIMD_INLINE batch<float, 16>::batch()
    {
    }

     XSIMD_INLINE batch<float, 16>::batch(const float* d)
        : base_type(vld4q_f32(d))
    {
    }

    XSIMD_INLINE batch<float, 16>::batch(const float* d, aligned_mode)
        : batch(d)
    {
    }

    XSIMD_INLINE batch<float, 16>::batch(const float* d, unaligned_mode)
        : batch(d)
    {
    }

    XSIMD_INLINE batch<float, 16>::batch(const storage_type& rhs)
        : base_type(rhs)
    {
    }

    XSIMD_INLINE batch<float, 16>& batch<float, 16>::operator=(const storage_type& rhs)
    {
        this->m_value = rhs;
        return *this;
    }

    XSIMD_INLINE float32x4_t batch<float, 16>::operator[](std::size_t index) const
    {
        return this->m_value.val[index];
    }

    XSIMD_INLINE batch<float, 16>::operator float32x4x4_t() const
    {
        return this->m_value;
    }
}

#endif
