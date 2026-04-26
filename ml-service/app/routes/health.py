from sqlalchemy import text
from sqlalchemy.orm import Session
from fastapi import APIRouter, Depends

from app.db import get_db

router = APIRouter(tags=["Health"])


@router.get("/health", summary="Healthcheck сервиса и БД")
def health(db: Session = Depends(get_db)):
    try:
        db.execute(text("SELECT 1"))
        db_status = "ok"
    except Exception as exc:
        db_status = f"error: {exc}"

    return {
        "status": "ok" if db_status == "ok" else "degraded",
        "database": db_status,
    }
