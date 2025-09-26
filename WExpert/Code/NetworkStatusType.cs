using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WExpert.Code;

public enum NetworkStatusType
{
    SUCCESS = 0,            // 성공
    HTTP_ERROR,             // HTTP 오류
    NETWORK_ERROR,          // 네트워크 오류
    ALREADY_REQUESTED,      // 이미 요청 중 
    EXCEEDING_MAX_REQUEST,  // 최대 요청 개수 초과
    INTERNAL_ERROR          // 내부 오류
};